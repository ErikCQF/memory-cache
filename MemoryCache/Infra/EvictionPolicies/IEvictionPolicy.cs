namespace MemoryCache.Infra.EvictionPolicies
{
    public interface IEvictionPolicy<TKey, TValue>
    {
        void EvictIfNeeded(IMemoryCache<TKey, TValue> dataStore);
    }
}
