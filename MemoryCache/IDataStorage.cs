using MemoryCache.Infra;

namespace MemoryCache
{
    public interface IDataStorage<TKey, TValue>
    {
        int Count { get; }
        void AddOrUpdate(TKey key, TValue value);
        bool Contains(TKey key);
        TValue? Get(TKey key);
        DataEnvolope<TKey, TValue>? LeasUsed();
        void Remove(TKey key);
    }
}