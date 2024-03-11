using MemoryCache.EvictionPolicies;
using MemoryCache.Infra;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reactive.Subjects;

namespace MemoryCache
{
    /// <summary>
    /// The cache should implement the ‘least recently used’ approach when selecting which item to evict.
    /// </summary>
    public sealed class DataStore<TKey, TValue> : IDataStore<TKey, TValue>
    {
        //For Settings
        public class DataStoreOptions
        {
            /// <summary>
            /// Provides the Max number of items the memory cache can hold
            /// </summary>
            public int Capacity { get; set; }
        }

        // For event-driven

        // private readonly Dictionary<TKey, Subject<DataStoreEvent<TKey>>> _dataItemsSubject = new Dictionary<TKey, Subject<DataStoreEvent<TKey>>>();

        //private readonly HashSet<DataEnvolope<TKey, Subject<DataStoreEvent<TKey>>>> _dataItemsSubjectSet = new HashSet<DataEnvolope<TKey, Subject<DataStoreEvent<TKey>>>>();

        private readonly Subject<DataStoreEvent<TKey>> _dataStoreSubject = new Subject<DataStoreEvent<TKey>>();

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


        // For Data Storage
        // TODO: It should be moved to a repo and the repos uses command and query separations. 
        // It will decoulpe the component and the storage.
        private readonly HashSet<DataEnvolope<TKey, TValue>> _dataHashSet = new HashSet<DataEnvolope<TKey, TValue>>();

        private readonly HashSet<DataItemObserver<TKey>> _subscribersSet = new HashSet<DataItemObserver<TKey>>();

        private readonly LinkedList<DataEnvolope<TKey, TValue>> _dataLinkedList = new LinkedList<DataEnvolope<TKey, TValue>>();

        private readonly ILogger<DataStore<TKey, TValue>> _logger;

        private readonly IOptions<DataStoreOptions> _options;

        private readonly IEnumerable<IEvictionPolicy<TKey, TValue>> _evictionPolicies;

        private object _lock = new object();

        public DataStore(ILogger<DataStore<TKey, TValue>> logger,
                         IOptions<DataStoreOptions> options,
                         IEnumerable<IEvictionPolicy<TKey, TValue>> evictionPolicies
                         )
        {
            this._logger = logger;
            this._options = options;
            this._evictionPolicies = evictionPolicies;
        }

        /// <summary>
        ///  need be thread safe
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _dataHashSet.Count;
                }
            }
        }

        public int Capacity
        {
            get
            {
                lock (_lock)
                {
                    return _options.Value.Capacity;
                }
            }
        }


        public void AddUpdate(TKey key, TValue value)
        {
            lock (_lock)
            {
                EvictIfNeeded();               
                AddOrUpdate(key, value);
            }
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
            lock (_lock)
            {
                TValue? val = default; ;
                var dataItem = new DataEnvolope<TKey, TValue>(key, val);
                DataEnvolope<TKey, TValue>? found;
                if (_dataHashSet.TryGetValue(dataItem, out found))
                {
                    _dataLinkedList.Remove(found);
                    _dataLinkedList.AddFirst(found);
                    return found.keyValuePair.Value;
                }
                return val;
            }

        }

        public DataEnvolope<TKey, TValue>? LeasUsed()
        {
            return _dataLinkedList?.Last?.Value;
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

            var dataItem = new DataEnvolope<TKey, TValue>(key, value);

            if (_dataHashSet.Contains(dataItem))
            {
                _dataHashSet.Remove(dataItem);
                _dataLinkedList.Remove(dataItem);
            }

            _dataHashSet.Add(dataItem);
            _dataLinkedList.AddFirst(dataItem);
        }

        public void Remove(TKey key)
        {
            lock (_lock)
            {
                var val = this.Get(key);
                var envolpe = new DataEnvolope<TKey, TValue>(key, val);
                _dataHashSet.Remove(envolpe);
                _dataLinkedList.Remove(envolpe);
            }

        }
    }

}