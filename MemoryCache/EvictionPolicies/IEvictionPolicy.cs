namespace MemoryCache.EvictionPolicies
{
    public interface IEvictionPolicy<TKey, TValue>
    {
        void EvictIfNeeded(IDataStoreBridge<TKey, TValue> dataStore);
    }
}
