using MemoryCache;
using MemoryCache.Infra.Events;
using MemoryCache.Infra.EvictionPolicies;

namespace TestMemoryCache.MemoryCache
{
    [Trait("Category", "MemoryCache")]

    public class MemoryCacheTests: MemoryCacheTests_Base
    {

   

        [Theory]
        [InlineData(5, 3, 3)]
        [InlineData(5, 5, 5)]
        [InlineData(5, 6, 5)]
        public void Add_Should_Add_Item_To_Cache(int capacity, int totalToAdd, int countResult)
        {
            // Arrange
            _optionsMock.SetupGet(o => o.Value).Returns(new MemoryCacheOptions { Capacity = capacity });

            // Add the implemented Eviction Policy strategy 
            _evictionPolices.Add(new EvictionStrategyLru<string, object>());

            var dataStore = new MemoryCache<string, object>(_loggerMock.Object, _optionsMock.Object, _evictionPolices, _dataStorage);

            // Act 
            // It will exceed the capacity by one
            for (int i = 0; i < totalToAdd; i++)
            {
                dataStore.AddUpdate(i.ToString(), i);
            }

            // Assert
            // can only have its capacity
            Assert.Equal(countResult, dataStore.Count);
        }

        [Fact]
        public void Add_Should_Evict_Least_Recently_ADDED_Item_When_Capacity_Is_Exceeded()
        {
            // Arrange
            _optionsMock.SetupGet(o => o.Value).Returns(new MemoryCacheOptions { Capacity = 2 });

            // Add the implemented Eviction Policy strategy 
            _evictionPolices.Add(new EvictionStrategyLru<string, object>());

            var dataStore = new MemoryCache<string, object>(_loggerMock.Object, _optionsMock.Object, _evictionPolices, _dataStorage);

            // Act
            dataStore.AddUpdate("key1", "value1");
            dataStore.AddUpdate("key2", "value2");
            dataStore.AddUpdate("key3", "value3");

            // Assert
            Assert.Null(dataStore.Get("key1")); // key1 should be evicted
            Assert.Equal("value2", dataStore.Get("key2")); // key2 should still be in cache
            Assert.Equal("value3", dataStore.Get("key3")); // key3 should still be in cache
            Assert.Equal(2, dataStore.Count); // Cache count should remain within capacity
        }

        [Fact]
        public void Add_Should_Evict_Least_Recently_USED_Item_When_Capacity_Is_Exceeded()
        {
            // Arrange

            _optionsMock.SetupGet(o => o.Value).Returns(new MemoryCacheOptions { Capacity = 3 });

            // Add the implemented Eviction Policy strategy 
            _evictionPolices.Add(new EvictionStrategyLru<string, object>());

            var dataStore = new MemoryCache<string, object>(_loggerMock.Object, _optionsMock.Object, _evictionPolices, _dataStorage);

            // Act
            dataStore.AddUpdate("key1", "value1");
            dataStore.AddUpdate("key2", "value2");
            dataStore.AddUpdate("key3", "value3");
            // key1 is not the last accessed anymore. it is key2 2
            dataStore.Get("key1");

            //Key 2 shluld be evicted
            dataStore.AddUpdate("key4", "value4");

            // Assert
            Assert.Null(dataStore.Get("key2")); // key1 should be evicted
            Assert.Equal("value3", dataStore.Get("key3")); // key2 should still be in cache
            Assert.Equal("value4", dataStore.Get("key4")); // key4 should still be in cache

            Assert.True(dataStore.Get("key2") is null); // key2 should be Evicted from the cache

            Assert.Equal(3, dataStore.Count); // Cache count should remain within capacity
        }
    }
}
