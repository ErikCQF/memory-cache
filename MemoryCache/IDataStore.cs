using MemoryCache.Infra;

namespace MemoryCache
{
    public interface IDataStore<TKey, TValue>
    {
        int Count { get; }
        int Capacity { get; }

        void Add(TKey key, TValue value);
        TValue? Get(TKey key);
        DataEnvolope<TKey, TValue>? LastUsed();
        void Remove(TKey key);
    }
}