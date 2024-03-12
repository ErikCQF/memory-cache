using System;
using System.Collections.Generic;

namespace MemoryCache.Infra.Storages
{
    /// <summary>
    /// This class represents an envelope for caching values, with optional metadata such as creation date and eviction priority.
    /// </summary>
    internal class DataEnvolope<TKey, TValue> : IEquatable<DataEnvolope<TKey, TValue>>
    {
        public readonly KeyValuePair<TKey, TValue?> KeyValuePair;

        public DataEnvolope(TKey key, TValue? value)
        {
            if (key == null)
            {
                throw new ArgumentException("Key must not be null.", nameof(KeyValuePair));
            }

            KeyValuePair = new KeyValuePair<TKey, TValue?>(key, value);
        }

        public bool Equals(DataEnvolope<TKey, TValue>? other)
        {
            if (other == null || other.KeyValuePair.Key == null)
            {
                return false;
            }

            if (KeyValuePair.Key == null)
            {
                return false;
            }

            return KeyValuePair.Key.Equals(other.KeyValuePair.Key);
        }

        public override int GetHashCode()
        {
            if (KeyValuePair.Key == null)
            {
                throw new ArgumentNullException($"{nameof(KeyValuePair)}. Key is null, and it must be a valid value to be used as a key.");
            }

            return KeyValuePair.Key.GetHashCode();
        }
    }
}
