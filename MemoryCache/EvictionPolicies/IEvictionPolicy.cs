namespace MemoryCache.EvictionPolicies
{
    public interface IEvictionPolicy<TKey, TValue>
    {
        void EvictIfNeeded(IDataStore<TKey, TValue> dataStore);
    }
}
