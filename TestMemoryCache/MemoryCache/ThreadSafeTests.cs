using MemoryCache.Infra.EvictionPolicies;
using MemoryCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMemoryCache.MemoryCache
{
    [Trait("Category", "Thread Safe")]
    public class ThreadSafeTests : MemoryCacheTests_Base
    {
        [Theory]
        [InlineData(10, 5, 11, 1)]
        [InlineData(10, 5, 11, 2)]
        [InlineData(10, 10, 1000, 1000)]

        public void Capacity_is_Changed_Must_Be_Thread_Safe(int capacity, int capacityNew, int numItemsPerThread, int numThreads)
        {
            //Arrange
            _optionsMock.SetupGet(o => o.Value).Returns(new MemoryCacheOptions { Capacity = capacity });

            // Add the implemented Eviction Policy strategy 
            _evictionPolices.Add(new EvictionStrategyLru<string, object>());

            var dataStore = new MemoryCache<string, object>(_loggerMock.Object, _optionsMock.Object, _evictionPolices, _dataStorage);

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
                        dataStore.AddUpdate(Guid.NewGuid().ToString(), ii * numItemsPerThread + j);

                    }
                });
            }

            Task.Delay(100).Wait();

            dataStore.SetCapacity(capacityNew);

            Task.WaitAll(tasks);

            // start a new execution
            // Concurrently add items
            tasks = new Task[numThreads];

            for (int i = 0; i < numThreads; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    int ii = i;
                    for (int j = 0; j < numItemsPerThread; j++)
                    {
                        dataStore.AddUpdate(Guid.NewGuid().ToString(), ii * numItemsPerThread + j);

                    }
                });
            }

            Task.WaitAll(tasks);


            //Assert
            //Must respect the capacity

            Assert.Equal(capacityNew, dataStore.Capacity);
            Assert.Equal(capacityNew, dataStore.Count);

        }



        [Theory]
        [InlineData(10, 100)]
        [InlineData(10, 10000)]
        public void Add_Same_Key_ShouldBe_ThreadSafe(int capacity, int numThreads)
        {
            //Arrange
            _optionsMock.SetupGet(o => o.Value).Returns(new MemoryCacheOptions { Capacity = capacity });

            // Add the implemented Eviction Policy strategy 
            _evictionPolices.Add(new EvictionStrategyLru<string, object>());

            var dataStore = new MemoryCache<string, object>(_loggerMock.Object, _optionsMock.Object, _evictionPolices, _dataStorage);

            var tasks = new Task[numThreads];


            // Acct
            // Concurrently add items
            for (int i = 0; i < numThreads; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    dataStore.AddUpdate("Same Key", "Same Value");

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
        [InlineData(10, 100, 100)]
        [InlineData(10, 1000, 5000)]
        public void Add_Should_Be_Thread_Safe(int capacity, int numItemsPerThread, int numThreads)
        {
            //Arrange
            _optionsMock.SetupGet(o => o.Value).Returns(new MemoryCacheOptions { Capacity = capacity });

            // Add the implemented Eviction Policy strategy 
            _evictionPolices.Add(new EvictionStrategyLru<string, object>());

            var dataStore = new MemoryCache<string, object>(_loggerMock.Object, _optionsMock.Object, _evictionPolices, _dataStorage);

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
                        dataStore.AddUpdate(Guid.NewGuid().ToString(), ii * numItemsPerThread + j);

                    }
                });
            }

            Task.WaitAll(tasks);


            //Assert
            //Must respect the capacity
            Assert.Equal(capacity, dataStore.Count);

        }
    }
}
