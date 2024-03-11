
namespace MemoryCache.Infra
{
    public class LruEvictionStrategy<TKey, TValue> : IEvictionPolicy<TKey, TValue>
    {
        public void EvictIfNeeded(DataStore<TKey, TValue> dataStore)
        {
            while (dataStore.Count >= dataStore.Capacity)
            {
                var last = dataStore.LastUsed();
                if (last != null)
                {                    
                    dataStore.Notify(last.keyValuePair.Key, DataStoreEventType.Evicted);
                    dataStore.Remove(last.keyValuePair.Key);
                }
            }
        }
    }
}
