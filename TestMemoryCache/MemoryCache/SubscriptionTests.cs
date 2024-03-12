using MemoryCache.Infra.Events;
using MemoryCache.Infra.EvictionPolicies;
using MemoryCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMemoryCache.MemoryCache
{
    [Trait("Category","Events")]
    public class SubscriptionTests : MemoryCacheTests_Base
    {
        [Fact]
        public void Subscribers_DataStore_Receive_Eviction_Events()
        {
            // Arrange
            _optionsMock.SetupGet(o => o.Value).Returns(new MemoryCacheOptions { Capacity = 2 });

            // Add the implemented Eviction Policy strategy 
            _evictionPolices.Add(new EvictionStrategyLru<string, object>());

            var dataStore = new MemoryCache<string, object>(_loggerMock.Object, _optionsMock.Object, _evictionPolices, _dataStorage);

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

            dataStore.AddUpdate(keyoBeEvicted, keyoBeEvicted);

            dataStore.AddUpdate("key2", "value2");
            dataStore.AddUpdate("key3", "value3"); // This should trigger eviction

            // Assert
            Assert.True(evictionEventReceived); // a event has been triggered
            Assert.True(evictionEventReceived_2); // a event has been triggered

            Assert.Equal(keyoBeEvicted, keyEvicted);  // the evicted key from the event is the key of the data that has been evicketd
            Assert.Equal(keyoBeEvicted_2, keyEvicted);  // the evicted key from the event is the key of the data that has been evicketd


            Assert.True(dataStore.Get(keyoBeEvicted) == null); // This evicted item does not exists anymore in the cache;
        }

        [Fact]
        public void Subscribers_To_DataItem_DataStore_Receive_Eviction_Events()
        {
            // Arrange
            _optionsMock.SetupGet(o => o.Value).Returns(new MemoryCacheOptions { Capacity = 2 });

            // Add the implemented Eviction Policy strategy 
            _evictionPolices.Add(new EvictionStrategyLru<string, object>());

            var dataStore = new MemoryCache<string, object>(_loggerMock.Object, _optionsMock.Object, _evictionPolices, _dataStorage);

            var evictionEventReceived = false;
            var keyoBeEvicted = "key1";

            var evictionEventReceived_2 = false;
            var keyoBeEvicted_2 = "key1";

            var keyEvicted = string.Empty;


            // Act

            dataStore.AddUpdate(keyoBeEvicted, keyoBeEvicted);

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

            dataStore.AddUpdate("key2", "value2");
            dataStore.AddUpdate("key3", "value3"); // This should trigger eviction

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
            _optionsMock.SetupGet(o => o.Value).Returns(new MemoryCacheOptions { Capacity = 2 });

            // Add the implemented Eviction Policy strategy 
            _evictionPolices.Add(new EvictionStrategyLru<string, object>());

            var dataStore = new MemoryCache<string, object>(_loggerMock.Object, _optionsMock.Object, _evictionPolices, _dataStorage);

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


            dataStore.AddUpdate(keyoBeEvicted, keyoBeEvicted);

            dataStore.DataItemEvents(keyoBeEvicted)
            .Subscribe(ev =>
            {
                if (ev.DataStoreEventType == DataStoreEventType.Evicted)
                {
                    evictionEventReceived_2 = true;
                    keyoBeEvicted_2 = ev.Key;
                }
            });

            dataStore.AddUpdate("key2", "value2");
            dataStore.AddUpdate("key3", "value3"); // This should trigger eviction

            // Assert
            Assert.True(evictionEventReceived); // a event has been triggered
            Assert.True(evictionEventReceived_2); // a event has been triggered

            Assert.Equal(keyoBeEvicted, keyEvicted);  // the evicted key from the event is the key of the data that has been evicketd
            Assert.Equal(keyoBeEvicted_2, keyEvicted);  // the evicted key from the event is the key of the data that has been evicketd

            Assert.True(dataStore.Get(keyoBeEvicted) == null); // This evicted item does not exists anymore in the cache;
            Assert.True(dataStore.Get(keyoBeEvicted_2) == null); // This evicted item does not exists anymore in the cache;

        }
    }
}
