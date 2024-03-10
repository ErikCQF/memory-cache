namespace MemoryCache
{
    /// <summary>
    /// This class will be used to envolep the caching value. It can have metadate such as creation date, eviction priority 
    /// There is a functional needs of data eviction. 
    /// </summary>
    public class DataEnvolope : IEquatable<DataEnvolope>
    {
        public readonly KeyValuePair<string, object> keyValuePair;

        public DataEnvolope(string key, object value)
        {

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key must be a non-empty string.", nameof(keyValuePair));
            }

            this.keyValuePair = new KeyValuePair<string, object>(key, value);
        }

        public virtual bool Equals(DataEnvolope? other)
        {
            if (other == null)
            {
                return false;
            }
            return other.keyValuePair.Key == this.keyValuePair.Key;
        }
        public override int GetHashCode()
        {
            return keyValuePair.Key.GetHashCode();
        }
    }

}