using MemoryCache.Infra.Events;

namespace MemoryCache
{
    public interface IMemoryCache<TKey, TValue>
    {
        int Count { get; }
        int Capacity { get; }
        void SetCapacity(int capacity);
        void AddUpdate(TKey key, TValue value);
        TValue? Get(TKey key);
        KeyValuePair<TKey, TValue>? LeasUsed();
        void Notify(TKey key, DataStoreEventType dataStoreEventType);
        void Remove(TKey key);
    }
}