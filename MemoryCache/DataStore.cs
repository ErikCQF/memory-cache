﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MemoryCache
{
    /// <summary>
    /// The cache should implement the ‘least recently used’ approach when selecting which item
    //  to evict.
    /// </summary>
    public class DataStore : IDataStore
    {

        public class DataStoreOptions
        {
            /// <summary>
            /// Provides the Max number of items the memory cache can hold
            /// </summary>
            public int Capacity { get; set; }
        }

        private object _lock = new object();

        private readonly HashSet<DataEnvolope> _dataHashSet = new HashSet<DataEnvolope>();        
        private readonly LinkedList<DataEnvolope> _dataLinkedList =  new  LinkedList<DataEnvolope>();

        private readonly ILogger<DataStore> _logger;
        private readonly IOptions<DataStoreOptions> _options;

        public DataStore(ILogger<DataStore> logger, IOptions<DataStoreOptions> options)
        {
            this._logger = logger;
            this._options = options;
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

        public void Add(string key, object value)
        {
            lock (_lock)
            {
                var dataItem = new DataEnvolope(key, value);


                while (Count >= _options.Value.Capacity)
                {
                    var last = _dataLinkedList.Last();
                    if (last != null)
                    {
                        _dataHashSet.Remove(last);
                        _dataLinkedList.Remove(last);
                    }
                }

                if (_dataHashSet.Contains(dataItem))
                {
                    _dataHashSet.Remove(dataItem);
                    _dataLinkedList.Remove(dataItem);                    
                    
                }
                _dataHashSet.Add(dataItem);
                _dataLinkedList.AddFirst(dataItem);

            }
        }

        /// <summary>
        /// The cache should implement the ‘least recently used’ approach when selecting which item to evict.
        /// It means that if the item is read, it left the last position of the quue and it goes to the head of the queuye
        /// </summary>


        public object? Get(string key)
        {
            lock (_lock)
            {
                var dataItem = new DataEnvolope(key, null);
                DataEnvolope? found;
                if (_dataHashSet.TryGetValue(dataItem, out found))
                {
                    _dataLinkedList.Remove(found);
                    _dataLinkedList.AddFirst(found);
                    return found.keyValuePair.Value;
                }
                return null;
            }
            
        }
    }

}