using MemoryCache;
using MemoryCache.Infra.Events;
using MemoryCache.Infra.EvictionPolicies;
using Moq;


namespace TestMemoryCache.EvictionStrategies
{
    [Trait("Category", "Eviction Strategy")]

    public class EvictionStrategyTests
    {
        /// <summary>
        /// Always when the capacity is reached. 
        /// </summary>
        [Fact]
        public void EvictIfNeeded_RemovesLeastUsedItem_WhenCapacityExceeded()
        {
            // Arrange
            int countItem = 3;
            int countExpecteed = 2;

            int capacity = 3;

            var dataStoreMock = new Mock<IMemoryCache<int, string>>();

            dataStoreMock.SetupGet(d => d.Capacity).Returns(capacity);
            dataStoreMock.Setup(d => d.Count).Returns(() => countItem);

            dataStoreMock.Setup(d => d.Remove(It.IsAny<int>()))
                .Callback<int>(key => countItem--); // Reduce by one the mock total Counts

            // Assume data store alreadycontains  items and we're removing the least recently used item
            var leastUsedItem = new KeyValuePair<int, string>(1, "Item1");

            dataStoreMock.Setup(d => d.LeasUsed()).Returns(leastUsedItem);

            var evictionStrategy = new EvictionStrategyLru<int, string>();

            // Act

            evictionStrategy.EvictIfNeeded(dataStoreMock.Object);

            // Assert
            // Need notify
            dataStoreMock.Verify(d => d.Notify(leastUsedItem.Key, DataStoreEventType.Evicted), Times.Once);
            // Need remove from datastore
            dataStoreMock.Verify(d => d.Remove(It.IsAny<int>()), Times.Once);
            // Verify that countItem is decremented when Remove method is called                       
            Assert.Equal(countExpecteed, countItem); // Check if countItem equals capacity
        }
    }
}
