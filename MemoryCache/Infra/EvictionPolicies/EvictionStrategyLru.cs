using MemoryCache.Infra.Events;

namespace MemoryCache.Infra.EvictionPolicies
{
    public class EvictionStrategyLru<TKey, TValue> : IEvictionPolicy<TKey, TValue>
    {
        public void EvictIfNeeded(IMemoryCache<TKey, TValue> dataStore)
        {
            while (dataStore.Count >= dataStore.Capacity)
            {
                var last = dataStore.LeasUsed();
                if (last != null)
                {
                    dataStore.Notify(last.Value.Key, DataStoreEventType.Evicted);
                    dataStore.Remove(last.Value.Key);
                }
            }
        }
    }
}
