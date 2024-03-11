using MemoryCache.Infra;

namespace MemoryCache
{
    public interface IDataStoreBridge<TKey, TValue>
    {
        int Count { get; }
        int Capacity { get; }

        void AddUpdate(TKey key, TValue value);
        TValue? Get(TKey key);
        DataEnvolope<TKey, TValue>? LeasUsed();
        void Notify(TKey key, DataStoreEventType dataStoreEventType);
        void Remove(TKey key);
    }
}