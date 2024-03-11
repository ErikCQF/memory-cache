namespace MemoryCache.Infra
{
    public interface IEvictionPolicy<TKey, TValue>
    {
        void EvictIfNeeded(DataStore<TKey, TValue> dataStore);
    }
}
