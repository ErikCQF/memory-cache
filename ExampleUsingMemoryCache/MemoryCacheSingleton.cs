using MemoryCache;
using MemoryCache.Infra.EvictionPolicies;
using MemoryCache.Infra.Storages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleUsingMemoryCache
{
    public class MemoryCacheSingleton<TKey, TValue>
    {

        private static readonly Lazy<MemoryCache<TKey, TValue>> lazyInstance =
            new Lazy<MemoryCache<TKey, TValue>>(() => new MemoryCache<TKey, TValue>(
                //Log
                new LoggerFactory().CreateLogger<MemoryCache<TKey, TValue>>(),
                //Options for initialization
                Options.Create(new MemoryCacheOptions() { Capacity = 1000}),
                //Eviction Polices
                new List<IEvictionPolicy<TKey, TValue>>([new EvictionStrategyLru<TKey, TValue>()]),
                //Data Structure used to store data in memory
                new DataStorage<TKey, TValue>()
            ));

        public static MemoryCache<TKey, TValue> Instance => lazyInstance.Value;
    }
    
}
