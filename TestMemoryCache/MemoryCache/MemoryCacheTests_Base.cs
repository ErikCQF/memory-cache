using MemoryCache;
using MemoryCache.Infra.EvictionPolicies;
using MemoryCache.Infra.Storages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace TestMemoryCache.MemoryCache
{
    public abstract class MemoryCacheTests_Base
    {
        protected readonly Mock<ILogger<MemoryCache<string, object>>> _loggerMock = new Mock<ILogger<MemoryCache<string, object>>>();
        protected readonly Mock<IOptions<MemoryCacheOptions>> _optionsMock = new Mock<IOptions<MemoryCacheOptions>>();

        protected readonly IDataStorage<string, object> _dataStorage = new DataStorage<string, object>();
        protected List<IEvictionPolicy<string, object>> _evictionPolices = new List<IEvictionPolicy<string, object>>();
    }
}
