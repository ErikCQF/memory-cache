namespace MemoryCache.Infra.EvictionPolicies
{
    public interface IEvictionPolicy<TKey, TValue>
    {
        void EvictIfNeeded(IDataStoreBridge<TKey, TValue> dataStore);
    }
}
