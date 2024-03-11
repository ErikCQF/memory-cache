using System.Collections.Generic;
using System.Reactive.Subjects;

namespace MemoryCache.Infra
{
    /// <summary>
    /// This class will be used to envolep the caching value. It can have metadate such as creation date, eviction priority 
    /// There is a functional needs of data eviction. 
    /// </summary>
    public class DataEnvolope<TKey, TValue> : IEquatable<DataEnvolope<TKey, TValue>>
    {
        public readonly KeyValuePair<TKey, TValue?> keyValuePair;
        public DataEnvolope(TKey key, TValue? value)
        {

            if (key == null)
            {
                throw new ArgumentException("Key must be a non-null.", nameof(keyValuePair));
            }

            keyValuePair = new KeyValuePair<TKey, TValue?>(key, value);
        }

        public bool Equals(DataEnvolope<TKey, TValue>? other)
        {
            if (other == null || other.keyValuePair.Key == null)
            {
                return false;
            }

            if (keyValuePair.Key == null)
            {
                return false;
            }

            return keyValuePair.Key.Equals(other.keyValuePair.Key);
        }

        public override int GetHashCode()
        {
            if (keyValuePair.Key == null)
            {
                throw new ArgumentNullException($"{nameof(keyValuePair)} . Key is null. must be a valid value to be used as a key");
            }
            return keyValuePair.Key.GetHashCode();
        }
    }
}