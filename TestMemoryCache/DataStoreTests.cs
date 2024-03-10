﻿using MemoryCache;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMemoryCache
{
    public class DataStoreTests
    {
        private readonly Mock<ILogger<DataStore>> loggerMock = new Mock<ILogger<DataStore>>();
        private readonly Mock<IOptions<DataStore.DataStoreOptions>> optionsMock = new Mock<IOptions<DataStore.DataStoreOptions>>();

        [Theory]
        [InlineData(5, 3, 3)]
        [InlineData(5, 5, 5)]
        [InlineData(5, 6, 5)]
        public void Add_Should_Add_Item_To_Cache(int capacity, int totalToAdd, int countResult)
        {
            // Arrange
            optionsMock.SetupGet(o => o.Value).Returns(new DataStore.DataStoreOptions { Capacity = capacity });
            var dataStore = new DataStore(loggerMock.Object, optionsMock.Object);

            // Act 
            // It will exceed the capacity by one
            for (int i = 0; i < totalToAdd; i++)
            {
                dataStore.Add(i.ToString(), i);
            }

            // Assert
            // can only have its capacity
            Assert.Equal(countResult, dataStore.Count);
        }

        [Fact]
        public void Add_Should_Evict_Least_Recently_ADDED_Item_When_Capacity_Is_Exceeded()
        {
            // Arrange
            optionsMock.SetupGet(o => o.Value).Returns(new DataStore.DataStoreOptions { Capacity = 2 });
            var dataStore = new DataStore(loggerMock.Object, optionsMock.Object);

            // Act
            dataStore.Add("key1", "value1");
            dataStore.Add("key2", "value2");
            dataStore.Add("key3", "value3");

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
            optionsMock.SetupGet(o => o.Value).Returns(new DataStore.DataStoreOptions { Capacity = 3 });
            var dataStore = new DataStore(loggerMock.Object, optionsMock.Object);

            // Act
            dataStore.Add("key1", "value1");
            dataStore.Add("key2", "value2");
            dataStore.Add("key3", "value3");
            // key1 is not the last accessed anymore. it is key2 2
            dataStore.Get("key1");

            //Key 2 shluld be evicted
            dataStore.Add("key4", "value4");

            // Assert
            Assert.Null(dataStore.Get("key2")); // key1 should be evicted
            Assert.Equal("value3", dataStore.Get("key3")); // key2 should still be in cache
            Assert.Equal("value4", dataStore.Get("key4")); // key4 should still be in cache

            Assert.True(dataStore.Get("key2") is null); // key2 should be Evicted from the cache

            Assert.Equal(3, dataStore.Count); // Cache count should remain within capacity
        }

    }
}
