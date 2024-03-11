using MemoryCache.Infra;

namespace MemoryCache.EvictionPolicies
{
    public class EvictionStrategyLru<TKey, TValue> : IEvictionPolicy<TKey, TValue>
    {
        public void EvictIfNeeded(IDataStoreBridge<TKey, TValue> dataStore)
        {
            while (dataStore.Count >= dataStore.Capacity)
            {
                var last = dataStore.LeasUsed();
                if (last != null)
                {
                    dataStore.Notify(last.keyValuePair.Key, DataStoreEventType.Evicted);
                    dataStore.Remove(last.keyValuePair.Key);
                }
            }
        }
    }
}
