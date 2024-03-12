namespace MemoryCache
{
    //For Settings
    public class MemoryCacheOptions
    {       
        private volatile int _capacity;
        public int Capacity
        {
            get { return _capacity; }
            set { _capacity = value; }
        }
    }
}