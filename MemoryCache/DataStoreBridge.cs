using MemoryCache.Infra;
using MemoryCache.Infra.EvictionPolicies;
using MemoryCache.Infra.Storages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reactive.Subjects;

namespace MemoryCache
{
    /// <summary>
    /// The cache should implement the ‘least recently used’ approach when selecting which item to evict.
    /// </summary>
    public sealed class DataStoreBridge<TKey, TValue> : IDataStoreBridge<TKey, TValue>
    {
        // Events
        private readonly Subject<DataStoreEvent<TKey>> _dataStoreSubject = new Subject<DataStoreEvent<TKey>>();
        private readonly HashSet<DataItemObserver<TKey>> _subscribersSet = new HashSet<DataItemObserver<TKey>>();
        public IObservable<DataStoreEvent<TKey>> DataStoreEvents => _dataStoreSubject;
        public IObservable<DataStoreEvent<TKey>> DataItemEvents(TKey key)
        {
            lock (_lock)
            {
                var dataItem = new DataItemObserver<TKey>(key);
                DataItemObserver<TKey>? found;
                if (_subscribersSet.TryGetValue(dataItem, out found))
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

        // Evictions Polices
        private readonly IEnumerable<IEvictionPolicy<TKey, TValue>> _evictionPolicies;

        // Data Storage
        private readonly IDataStorage<TKey, TValue> _dataStorage;

        //Settings, logs and others
        private readonly ILogger<DataStoreBridge<TKey, TValue>> _logger;

        private readonly IOptions<DataStoreOptions> _options;

        private object _lock = new object();

        public DataStoreBridge(ILogger<DataStoreBridge<TKey, TValue>> logger,
                         IOptions<DataStoreOptions> options,
                         IEnumerable<IEvictionPolicy<TKey, TValue>> evictionPolicies,
                         IDataStorage<TKey, TValue> dataStorage
                         )
        {
            this._logger = logger;
            this._options = options;
            this._evictionPolicies = evictionPolicies;
            this._dataStorage = dataStorage;
        }

        /// <summary>
        ///  need be thread safe
        /// </summary>
        public int Count
        {
            get
            {
                return _dataStorage.Count;
            }
        }

        public int Capacity
        {
            get
            {
                return _options.Value.Capacity;
            }
        }


        public void AddUpdate(TKey key, TValue value)
        {
            EvictIfNeeded();
            AddOrUpdate(key, value);
        }
        public void Notify(TKey key, DataStoreEventType dataStoreEventType)
        {
            _dataStoreSubject?.OnNext(new DataStoreEvent<TKey>(key, dataStoreEventType));
            DataItemObserver<TKey>? found = new DataItemObserver<TKey>(key);
            if (_subscribersSet.TryGetValue(found, out found))
            {
                found.NotifyChanged(dataStoreEventType);
            }

        }

        /// <summary>
        /// The cache should implement the ‘least recently used’ approach when selecting which item to evict.
        /// It means that if the item is read, it left the last position of the quue and it goes to the head of the queuye
        /// </summary>
        public TValue? Get(TKey key)
        {
            return _dataStorage.Get(key);
        }

        public DataEnvolope<TKey, TValue>? LeasUsed()
        {
            return _dataStorage.LeasUsed();
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

        public void Remove(TKey key)
        {
            _dataStorage.Remove(key);
        }
    }

}