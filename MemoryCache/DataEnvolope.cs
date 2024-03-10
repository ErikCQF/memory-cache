namespace MemoryCache
{
    /// <summary>
    /// This class will be used to envolep the caching value. It can have metadate such as creation date, eviction priority 
    /// There is a functional needs of data eviction. 
    /// </summary>
    public class DataEnvolope<Key, Value> : IEquatable<DataEnvolope<Key, Value>>
    {
        public readonly KeyValuePair<Key, Value?> keyValuePair;

        public DataEnvolope(Key key, Value? value)
        {

            if (key == null)
            {
                throw new ArgumentException("Key must be a non-null.", nameof(keyValuePair));
            }

            this.keyValuePair = new KeyValuePair<Key, Value?>(key, value);
        }

        public bool Equals(DataEnvolope<Key, Value>? other)
        {
            if (other == null || other.keyValuePair.Key == null)
            {
                return false;
            }

            if (this.keyValuePair.Key == null)
            {
                return false;
            }

            return this.keyValuePair.Key.Equals(other.keyValuePair.Key);
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