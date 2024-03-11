# Functional requirements for Generic Memory Component

## Mandatory:

- A generic in-memory cache component. [&#x2705; done]
- Developers can use it in their applications. [&#x2705; done]
- The cache should implement the 'least recently used' approach for evictions when the capacity is reached. [&#x2705; done]
- The cache component is intended to be used as a singleton, ensuring thread safety. [&#x2705; done]

## Desireblle:

- Mechanism which allows the consumer to know when items get evicted. [&#x2705; done]

## Architecture

- `DataStoreBridge<TKey, TValue>` implements `IDataStoreBridge<TKey, TValue>`.
- `DataStoreBridge<TKey, TValue>` depends on "n" `IEvictionPolicy<TKey, TValue>`.
- `DataEnvolope<TKey, TValue>` is used to encapsulate `TValue`. It implements `IEquatable`, so a generic `TKEY` can be used for indexing. Another important point is that this approach makes it easier for Extensions.
- `DataItemObserver<TKey>` is used for Reactive Programming to notify evictions. It is also ready to notify other events such as updates; the event message can be extended to carry new information as well, such as delta in updates.

## Refactory: [TODO]

- It is a test version focused on delivering all functionalities requested. The refactoring is desirable for `DataStoreBridge<TKey, TValue>`. This interface is not following the single responsibility principle, so it needs to be broken down.
- Thread Safe. It is using a lock approach. Apply non locking design patterns when applyable.
- Regarding `DataStoreBridge` implementation, it depends on 2 `HashSet` and a linked list to implement the 'least recently used' approach for evictions. My suggestion is to decouple it using a CQRS approach, thereby decoupling queries and commands.

