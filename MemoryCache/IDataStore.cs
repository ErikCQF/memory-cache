using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MemoryCache
{
    public interface IDataStore
    {        
        void Add(string key, object value);
        object? Get(string key);             
    }
}