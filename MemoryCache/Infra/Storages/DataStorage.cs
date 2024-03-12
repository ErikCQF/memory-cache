using System.Collections.Generic;

namespace MemoryCache.Infra.Storages
{
    public class DataStorage<TKey, TValue> : IDataStorage<TKey, TValue>
    {
        private readonly HashSet<DataEnvolope<TKey, TValue>> _dataHashSet = new HashSet<DataEnvolope<TKey, TValue>>();
        private readonly LinkedList<DataEnvolope<TKey, TValue>> _dataLinkedList = new LinkedList<DataEnvolope<TKey, TValue>>();
        private readonly object _lock = new object();

        public TValue? Get(TKey key)
        {
            lock (_lock)
            {
                TValue? val = default;
                var dataItem = new DataEnvolope<TKey, TValue>(key, val);
                DataEnvolope<TKey, TValue>? found;
                if (_dataHashSet.TryGetValue(dataItem, out found))
                {
                    _dataLinkedList.Remove(found);
                    _dataLinkedList.AddFirst(found);
                    return found.KeyValuePair.Value;
                }
                return val;
            }
        }

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

        public bool Contains(TKey key)
        {
            lock (_lock)
            {
                var dataItem = new DataEnvolope<TKey, TValue>(key, default);
                return _dataHashSet.Contains(dataItem);
            }
        }

        public void AddOrUpdate(TKey key, TValue value)
        {
            lock (_lock)
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
        }

        public void Remove(TKey key)
        {
            lock (_lock)
            {
                var val = Get(key);
                var envelope = new DataEnvolope<TKey, TValue>(key, val);
                _dataHashSet.Remove(envelope);
                _dataLinkedList.Remove(envelope);
            }
        }

        public KeyValuePair<TKey, TValue?>? LeasUsed()
        {
            lock (_lock)
            {
                return _dataLinkedList?.Last?.Value?.KeyValuePair;
            }
        }
    }
}
