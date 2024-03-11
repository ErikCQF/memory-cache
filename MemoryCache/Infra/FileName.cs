//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace MemoryCache.Infra
//{
//    public interface IEvictionPolicy<TKey, TValue>
//    {
//        void EvictIfNeeded(DataStore<TKey, TValue> dataStore);
//    }
//    public class LruEvictionStrategy<TKey, TValue> : IEvictionPolicy<TKey, TValue>
//    {
//        public void EvictIfNeeded(DataStore<TKey, TValue> dataStore)
//        {
//            while (dataStore.DataHashSet.Count >= dataStore.Options.Capacity)
//            {
//                var last = dataStore.DataLinkedList.Last;
//                if (last != null)
//                {
//                    dataStore.DataHashSet.Remove(last.Value);
//                    dataStore.DataLinkedList.RemoveLast();
//                    dataStore.Notify(last.Value.keyValuePair.Key, DataStoreEventType.Evicted);
//                }
//            }
//        }
//    }
//}
