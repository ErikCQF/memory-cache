using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MemoryCache
{
    public interface IDataStore<Key, Value>
    {        
        void Add(Key key, Value value);
        Value? Get(Key key);             
    }
}