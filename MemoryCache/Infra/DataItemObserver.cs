using System.Collections.Generic;
using System.Reactive.Subjects;

namespace MemoryCache.Infra
{
    public class DataItemObserver<TKey> : IEquatable<DataItemObserver<TKey>>
    {

        private readonly TKey _key;

        public DataItemObserver(TKey key)
        {
            _key = key;
        }


        private readonly Subject<DataStoreEvent<TKey>> _dataStoreSubject = new Subject<DataStoreEvent<TKey>>();

        public IObservable<DataStoreEvent<TKey>> DataStoreEvents => _dataStoreSubject;

        public bool Equals(DataItemObserver<TKey> other)
        {
            if (other == null)
            {
                return false;
            }

            if (_key == null)
            {
                return false;
            }

            return _key.Equals(other._key);
        }

        public override bool Equals(object obj)
        {
            if (obj is DataItemObserver<TKey> other)
            {
                return Equals(other);
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (_key == null)
            {
                throw new ArgumentNullException($"{nameof(_key)} . Key is null. It must be a valid value to be used as a key.");
            }
            return _key.GetHashCode();
        }

        public virtual void NotifyChanged(DataStoreEventType dataStoreEventType)
        {
            _dataStoreSubject?.OnNext(new DataStoreEvent<TKey>(_key, dataStoreEventType));
        }
    }
}
