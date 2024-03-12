using MemoryCache.Infra.Events;
using MemoryCache.Infra.EvictionPolicies;
using MemoryCache.Infra.Storages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Linq;

namespace MemoryCache
{
    /// <summary>
    /// The cache should implement the 'least recently used' approach when selecting which item to evict.
    /// </summary>
    public sealed class MemoryCache<TKey, TValue> : IMemoryCache<TKey, TValue>
    {
        // Events
        private readonly Subject<DataStoreEvent<TKey>> _dataStoreSubject = new Subject<DataStoreEvent<TKey>>();
        private readonly HashSet<DataItemObserver<TKey>> _subscribersSet = new HashSet<DataItemObserver<TKey>>();
        private readonly object _lock = new object();

        public IObservable<DataStoreEvent<TKey>> DataStoreEvents => _dataStoreSubject;

        public IObservable<DataStoreEvent<TKey>> DataItemEvents(TKey key)
        {
            lock (_lock)
            {
                var dataItem = new DataItemObserver<TKey>(key);
                if (_subscribersSet.TryGetValue(dataItem, out var found))
                {
                    return found.DataStoreEvents;
                }
                else
                {
                    _subscribersSet.Add(dataItem);
                    return dataItem.DataStoreEvents;
                }
            }
        }

        // Evictions Policies
        private readonly IEnumerable<IEvictionPolicy<TKey, TValue>> _evictionPolicies;

        // Data Storage
        private readonly IDataStorage<TKey, TValue> _dataStorage;

        // Settings, Logs, and Others
        private readonly ILogger<MemoryCache<TKey, TValue>> _logger;
        private readonly IOptions<MemoryCacheOptions> _options;

        public MemoryCache(
            ILogger<MemoryCache<TKey, TValue>> logger,
            IOptions<MemoryCacheOptions> options,
            IEnumerable<IEvictionPolicy<TKey, TValue>> evictionPolicies,
            IDataStorage<TKey, TValue> dataStorage)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _evictionPolicies = evictionPolicies ?? throw new ArgumentNullException(nameof(evictionPolicies));
            _dataStorage = dataStorage ?? throw new ArgumentNullException(nameof(dataStorage));
        }

        /// <summary>
        ///  Thread-safe property to get the count of items in the cache.
        /// </summary>
        public int Count => _dataStorage.Count;

        /// <summary>
        /// Thread-safe property to get the capacity of the cache.
        /// </summary>
        public int Capacity => _options.Value.Capacity;

        /// <summary>
        /// Adds or updates an item in the cache.
        /// </summary>
        public void AddUpdate(TKey key, TValue value)
        {
            lock (_lock)
            {
                EvictIfNeeded();
                AddOrUpdate(key, value);
            }
        }

        /// <summary>
        /// Notifies subscribers about a data store event.
        /// </summary>
        public void Notify(TKey key, DataStoreEventType dataStoreEventType)
        {
            _dataStoreSubject?.OnNext(new DataStoreEvent<TKey>(key, dataStoreEventType));
            var found = new DataItemObserver<TKey>(key);
            if (_subscribersSet.TryGetValue(found, out var subscriber))
            {
                subscriber.NotifyChanged(dataStoreEventType);
            }
        }

        /// <summary>
        /// Retrieves an item from the cache.
        /// </summary>
        public TValue? Get(TKey key) => _dataStorage.Get(key);

        /// <summary>
        /// Retrieves the least used item from the cache.
        /// </summary>
        public KeyValuePair<TKey, TValue?>? LeasUsed() => _dataStorage?.LeasUsed();

        /// <summary>
        /// Removes an item from the cache.
        /// </summary>
        public void Remove(TKey key) => _dataStorage.Remove(key);

        /// <summary>
        /// Sets the capacity of the cache.
        /// </summary>
        public void SetCapacity(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");
            }

            _options.Value.Capacity = capacity;
        }

        private void EvictIfNeeded()
        {
            if (_evictionPolicies.Any())
            {
                foreach (var policy in _evictionPolicies)
                {
                    policy.EvictIfNeeded(this);
                }
            }
        }

        private void AddOrUpdate(TKey key, TValue value)
        {
            _dataStorage.AddOrUpdate(key, value);
        }
    }
}
