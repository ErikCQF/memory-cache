using ExampleUsingMemoryCache;
using MemoryCache;
using MemoryCache.Infra.Events;
using MemoryCache.Infra.EvictionPolicies;
using MemoryCache.Infra.Storages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

class Program
{
    static void Main(string[] args)
    {
        
        Console.WriteLine("**** Examples singleton *********");

        Using_MemoryCache_As_Singleton();

        Console.WriteLine("**** Examples instatiating *********");
        Instatiate_MemoryCache();

        // Wait for user input before closing the console window
        Console.ReadLine();
    }

    private static void Instatiate_MemoryCache()
    {
        //logger
        var logger = new LoggerFactory().CreateLogger<MemoryCache<int, string>>();

        //initial settings
        int capacities = 100;
        var options = Options.Create(new MemoryCacheOptions() { Capacity = capacities });

        // Evictions Algorithms are decoupled from memory cache
        // So others can be added or replace the existent one
        var evictionPolices = new List<IEvictionPolicy<int, string>>([new EvictionStrategyLru<int, string>()]);

        //Data Storage
        //How data is stored is decoupled from the memory cache
        IDataStorage<int, string> dataStorage = new DataStorage<int, string>();

        //Initializate
        var memoryCache = new MemoryCache<int, string>(
                            logger,
                            options,
                            evictionPolices,
                            dataStorage
                            );


        //adding items
        memoryCache.AddUpdate(1, "value");
        memoryCache.AddUpdate(2, "value");

        //removing an existent
        memoryCache.Remove(2);

        //remove an inexistent does not causes errors
        memoryCache.Remove(3);

        //Subscribe to all events
        memoryCache.DataStoreEvents.Subscribe(e => Console.WriteLine($"Data store event: {e.DataStoreEventType} for key: {e.Key}"));

        //Subscribe to the Data item Events
        int KeyToSubscribe = 10;
        memoryCache.DataItemEvents(KeyToSubscribe)
            .Subscribe(e => Console.WriteLine($"Data store event: {e.DataStoreEventType} for key: {e.Key}"));

        // Notify Subscribers
        memoryCache.Notify(11, DataStoreEventType.Evicted);

    }
    private static void Using_MemoryCache_As_Singleton()
    {
        var cache = MemoryCacheSingleton<string, int>.Instance;

        // Adding some data to the cache
        cache.AddUpdate("key1", 10);
        cache.AddUpdate("key2", 20);
        cache.AddUpdate("key3", 30);

        // Retrieving data from the cache
        Console.WriteLine("Value for key 'key1': " + cache.Get("key1")); // Output: 10
        Console.WriteLine("Value for key 'key2': " + cache.Get("key2")); // Output: 20
        Console.WriteLine("Value for key 'key3': " + cache.Get("key3")); // Output: 30

        // Removing data from the cache
        cache.Remove("key2");

        // Retrieving data again
        Console.WriteLine("Value for key 'key2' after removal: " + cache.Get("key2")); // Output: null

        // Changing capacity of the cache
        cache.SetCapacity(100);

        // Accessing other properties and methods of the cache
        Console.WriteLine("Current count: " + cache.Count);
        Console.WriteLine("Current capacity: " + cache.Capacity);

        // Notify
        cache.Notify("key1", DataStoreEventType.Evicted);

        cache.DataStoreEvents.Subscribe(e => Console.WriteLine($"Data store event: {e.DataStoreEventType} for key: {e.Key}"));
    }
}
