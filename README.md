# Functional requirements for A Generic Memory Component

## Mandatory:

- A generic in-memory cache component. [&#x2705; done]
- Developers can use it in their applications. [&#x2705; done]
- The cache should implement the 'least recently used' approach for evictions when the capacity is reached. [&#x2705; done]
- The cache component is intended to be used as a singleton, ensuring thread safety. [&#x2705; done]

## Desirable:

- Mechanism which allows the consumer to know when items get evicted. [&#x2705; done]

## Architecture

- `MemoryCache<TKey, TValue>` implements `MemoryCache<TKey, TValue>`.
  
- `MemoryCache<TKey, TValue>` depends on "n" `IEvictionPolicy<TKey, TValue>`.
  The Eviction Algorithm has been decoupled from MemoryCache.
  1 to N implementations of Eviction algorithms can be added. The one that is implemented is Least Recently Used.
  `IEvictionPolicy<TKey, TValue>` is decoupled from the Data Structure for storing data, adhering to Solid principles.

- `MemoryCache<TKey, TValue>` depends on "n" `IDataStorage<TKey, TValue>`.
  Data storing is decoupled from Memory cache. It uses an implementation of `IDataStorage<TKey, TValue>`.

- `DataEnvolope<TKey, TValue>` is used to encapsulate `TValue`. It implements `IEquatable`, so a generic `TKEY` can be used for indexing. Another important point is that this approach makes it easier for Extensions.

- `DataItemObserver<TKey>` is used for Reactive Programming to notify evictions. It is also ready to notify other events such as updates; the event message can be extended to carry new information as well, such as delta in updates.


## Review and Refactory: [TODO]

- At first view there is so many responsiblities to MemoryCache. Apply CQRS for data command and query. 
- *Thread Safe*. It is using a lock approach. Apply non locking design patterns when it is possible.
- Regarding `MemoryCache` implementation. It need to decouple the DataStorage [&#x2705; done] and the Eviction Algorithm [&#x2705; done] 
- Review Events, Risk of not disposing properly as it uses observable.

## Instantiate and Using Memory Cache
```
// Logger
var logger = new LoggerFactory().CreateLogger<MemoryCache<int, string>>();

// Initial Settings
int capacities = 100;
var options = Options.Create(new MemoryCacheOptions() { Capacity = capacities });

// Evictions Algorithms are decoupled from memory cache
// So others can be added or replace the existent one
var evictionPolices = new List<IEvictionPolicy<int, string>>([new EvictionStrategyLru<int, string>()]);

// Data Storage
// How data is stored is decoupled from the memory cache
IDataStorage<int, string> dataStorage = new DataStorage<int, string>();

// Initialize
var memoryCache = new MemoryCache<int, string>(
    logger,
    options,
    evictionPolices,
    dataStorage
);

// Adding Items
memoryCache.AddUpdate(1, "value");
memoryCache.AddUpdate(2, "value");

// Removing an existing item
memoryCache.Remove(2);

// Removing an inexistent item does not cause errors
memoryCache.Remove(3);

// Subscribe to all events
memoryCache.DataStoreEvents.Subscribe(e => Console.WriteLine($"Data store event: {e.DataStoreEventType} for key: {e.Key}"));

// Subscribe to the Data item Events
int KeyToSubscribe = 10;
memoryCache.DataItemEvents(KeyToSubscribe)
    .Subscribe(e => Console.WriteLine($"Data store event: {e.DataStoreEventType} for key: {e.Key}"));

// Notify Subscribers
memoryCache.Notify(11, DataStoreEventType.Evicted);
```
