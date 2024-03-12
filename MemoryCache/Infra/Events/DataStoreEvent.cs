namespace MemoryCache.Infra.Events
{
    public class DataStoreEvent<TKey>
    {
        public readonly TKey Key;
        public readonly DataStoreEventType DataStoreEventType;
        public DataStoreEvent(TKey key, DataStoreEventType dataStoreEventType)
        {
            Key = key;
            DataStoreEventType = dataStoreEventType;
        }
    }
}