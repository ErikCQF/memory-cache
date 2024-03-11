﻿using MemoryCache;
using MemoryCache.Infra;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMemoryCache
{

    public class DataStoreTests
    {
        private readonly Mock<ILogger<DataStore<string, object>>> loggerMock = new Mock<ILogger<DataStore<string, object>>>();
        private readonly Mock<IOptions<DataStore<string, object>.DataStoreOptions>> optionsMock = new Mock<IOptions<DataStore<string, object>.DataStoreOptions>>();

        [Fact]
        public void Subscribers_DataStore_Receive_Eviction_Events()
        {
            // Arrange
            optionsMock.SetupGet(o => o.Value).Returns(new DataStore<string, object>.DataStoreOptions { Capacity = 2 });
            var dataStore = new DataStore<string, object>(loggerMock.Object, optionsMock.Object);

            var evictionEventReceived = false;
            var keyoBeEvicted = "value1";

            var evictionEventReceived_2 = false;
            var keyoBeEvicted_2 = "value1";

            var keyEvicted = string.Empty;

            // Act
            dataStore.DataStoreEvents.Subscribe(ev =>
            {
                if (ev.DataStoreEventType == DataStoreEventType.Evicted)
                {
                    evictionEventReceived = true;
                    keyEvicted = ev.Key;
                }
            });
            dataStore.DataStoreEvents.Subscribe(ev =>
            {
                if (ev.DataStoreEventType == DataStoreEventType.Evicted)
                {
                    evictionEventReceived_2 = true;
                    keyoBeEvicted_2 = ev.Key;
                }
            });

            dataStore.Add(keyoBeEvicted, keyoBeEvicted);

            dataStore.Add("key2", "value2");
            dataStore.Add("key3", "value3"); // This should trigger eviction

            // Assert
            Assert.True(evictionEventReceived); // a event has been triggered
            Assert.True(evictionEventReceived_2); // a event has been triggered

            Assert.Equal(keyoBeEvicted, keyEvicted);  // the evicted key from the event is the key of the data that has been evicketd
            Assert.Equal(keyoBeEvicted_2, keyEvicted);  // the evicted key from the event is the key of the data that has been evicketd


            Assert.True(dataStore.Get(keyoBeEvicted)==null); // This evicted item does not exists anymore in the cache;
        }

        [Fact]
        public void Subscribers_To_DataItem_DataStore_Receive_Eviction_Events()
        {
            // Arrange
            optionsMock.SetupGet(o => o.Value).Returns(new DataStore<string, object>.DataStoreOptions { Capacity = 2 });
            var dataStore = new DataStore<string, object>(loggerMock.Object, optionsMock.Object);

            var evictionEventReceived = false;
            var keyoBeEvicted = "key1";

            var evictionEventReceived_2 = false;
            var keyoBeEvicted_2 = "key1";

            var keyEvicted = string.Empty;


            // Act
            
            dataStore.Add(keyoBeEvicted, keyoBeEvicted);
            
            dataStore.DataItemEvents(keyoBeEvicted)
            .Subscribe(ev =>
            {
                if (ev.DataStoreEventType == DataStoreEventType.Evicted)
                {
                    evictionEventReceived = true;
                    keyEvicted = ev.Key;
                }
            });

            dataStore.DataItemEvents(keyoBeEvicted)
            .Subscribe(ev =>
            {
                if (ev.DataStoreEventType == DataStoreEventType.Evicted)
                {
                    evictionEventReceived_2 = true;
                    keyoBeEvicted_2 = ev.Key;
                }
            });

            dataStore.Add("key2", "value2");
            dataStore.Add("key3", "value3"); // This should trigger eviction

            // Assert
            Assert.True(evictionEventReceived); // a event has been triggered
            Assert.True(evictionEventReceived_2); // a event has been triggered

            Assert.Equal(keyoBeEvicted, keyEvicted);  // the evicted key from the event is the key of the data that has been evicketd
            Assert.Equal(keyoBeEvicted_2, keyEvicted);  // the evicted key from the event is the key of the data that has been evicketd

            Assert.True(dataStore.Get(keyoBeEvicted) == null); // This evicted item does not exists anymore in the cache;
            Assert.True(dataStore.Get(keyoBeEvicted_2) == null); // This evicted item does not exists anymore in the cache;

        }


        [Fact]
        public void Subscribers_To_DataItem_Before_It_has_been_Added_DataStore_Receive_Eviction_Events()
        {
            // Arrange
            optionsMock.SetupGet(o => o.Value).Returns(new DataStore<string, object>.DataStoreOptions { Capacity = 2 });
            var dataStore = new DataStore<string, object>(loggerMock.Object, optionsMock.Object);

            var evictionEventReceived = false;
            var keyoBeEvicted = "key1";

            var evictionEventReceived_2 = false;
            var keyoBeEvicted_2 = "key1";

            var keyEvicted = string.Empty;

            // Act
           // this event has been subscribed before the item has been added to be evicted
           // if the item has been evicted, must be called.
            dataStore.DataItemEvents(keyoBeEvicted)
            .Subscribe(ev =>
            {
                if (ev.DataStoreEventType == DataStoreEventType.Evicted)
                {
                    evictionEventReceived = true;
                    keyEvicted = ev.Key;
                }
            });


            dataStore.Add(keyoBeEvicted, keyoBeEvicted);

            dataStore.DataItemEvents(keyoBeEvicted)
            .Subscribe(ev =>
            {
                if (ev.DataStoreEventType == DataStoreEventType.Evicted)
                {
                    evictionEventReceived_2 = true;
                    keyoBeEvicted_2 = ev.Key;
                }
            });

            dataStore.Add("key2", "value2");
            dataStore.Add("key3", "value3"); // This should trigger eviction

            // Assert
            Assert.True(evictionEventReceived); // a event has been triggered
            Assert.True(evictionEventReceived_2); // a event has been triggered

            Assert.Equal(keyoBeEvicted, keyEvicted);  // the evicted key from the event is the key of the data that has been evicketd
            Assert.Equal(keyoBeEvicted_2, keyEvicted);  // the evicted key from the event is the key of the data that has been evicketd

            Assert.True(dataStore.Get(keyoBeEvicted) == null); // This evicted item does not exists anymore in the cache;
            Assert.True(dataStore.Get(keyoBeEvicted_2) == null); // This evicted item does not exists anymore in the cache;

        }

        [Theory]
        [InlineData(10, 100)]
        public void Add_Same_Key_ShouldBe_ThreadSafe(int capacity, int numThreads)
        {
            //Arrange
            optionsMock.SetupGet(o => o.Value).Returns(new DataStore<string, object>.DataStoreOptions { Capacity = capacity });
            var dataStore = new DataStore<string, object>(loggerMock.Object, optionsMock.Object);
            var tasks = new Task[numThreads];


            // Acct
            // Concurrently add items
            for (int i = 0; i < numThreads; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    dataStore.Add("Same Key", "Same Value");

                });
            }

            Task.WaitAll(tasks);


            //Assert
            //Must have only a key
            Assert.Equal(1, dataStore.Count);
        }

        [Theory]
        [InlineData(10, 11, 1)]
        [InlineData(10, 11, 2)]
        [InlineData(10, 10, 1)]
        [InlineData(10, 10, 2)]
        [InlineData(10, 100, 100)]
        [InlineData(10, 100, 1000)]
        public void Add_Should_Be_Thread_Safe(int capacity, int numItemsPerThread, int numThreads)
        {
            //Arrange
            optionsMock.SetupGet(o => o.Value).Returns(new DataStore<string, object>.DataStoreOptions { Capacity = capacity });
            var dataStore = new DataStore<string, object>(loggerMock.Object, optionsMock.Object);
            var tasks = new Task[numThreads];

            // Acct
            // Concurrently add items
            for (int i = 0; i < numThreads; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    int ii = i;
                    for (int j = 0; j < numItemsPerThread; j++)
                    {
                        dataStore.Add(Guid.NewGuid().ToString(), ii * numItemsPerThread + j);

                    }
                });
            }

            Task.WaitAll(tasks);


            //Assert
            //Must respect the capacity
            Assert.Equal(capacity, dataStore.Count);

        }


        //[Theory]
        //[InlineData(10, 11, 1)]
        //[InlineData(10, 11, 2)]
        //[InlineData(10, 100, 100)]
        //[InlineData(10, 100, 1000)]
        //public void Add_RepetedKey_Should_Be_Thread_Safe(int capacity, int numItemsPerThread, int numThreads)
        //{
        //    //Arrange
        //    optionsMock.SetupGet(o => o.Value).Returns(new DataStore<string, object>.DataStoreOptions { Capacity = capacity });
        //    var dataStore = new DataStore<string, object>(loggerMock.Object, optionsMock.Object);
        //    var tasks = new Task[numThreads];

        //    //Acct
        //    // Concurrently add items
        //    for (int i = 0; i < numThreads; i++)
        //    {
        //        tasks[i] = Task.Run(() =>
        //        {
        //            for (int j = 0; j < numItemsPerThread; j++)
        //            {
        //                dataStore.Add($"Key{j}", j * numItemsPerThread);
        //            }
        //        });
        //    }

        //    Task.WaitAll(tasks);


        //    //Assert
        //    Assert.Equal(capacity, dataStore.Count);

        //}


        [Theory]
        [InlineData(5, 3, 3)]
        [InlineData(5, 5, 5)]
        [InlineData(5, 6, 5)]
        public void Add_Should_Add_Item_To_Cache(int capacity, int totalToAdd, int countResult)
        {
            // Arrange
            optionsMock.SetupGet(o => o.Value).Returns(new DataStore<string, object>.DataStoreOptions { Capacity = capacity });

            var dataStore = new DataStore<string, object>(loggerMock.Object, optionsMock.Object);

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

            optionsMock.SetupGet(o => o.Value).Returns(new DataStore<string, object>.DataStoreOptions { Capacity = 2 });

            var dataStore = new DataStore<string, object>(loggerMock.Object, optionsMock.Object);

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

            optionsMock.SetupGet(o => o.Value).Returns(new DataStore<string, object>.DataStoreOptions { Capacity = 3 });

            var dataStore = new DataStore<string, object>(loggerMock.Object, optionsMock.Object);

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