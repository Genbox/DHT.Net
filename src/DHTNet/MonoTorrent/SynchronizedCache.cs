namespace DHTNet.MonoTorrent
{
    internal class SynchronizedCache<T> : ICache<T>
    {
        private readonly ICache<T> _cache;

        public SynchronizedCache(ICache<T> cache)
        {
            Check.Cache(cache);
            this._cache = cache;
        }

        public int Count
        {
            get { return _cache.Count; }
        }

        public T Dequeue()
        {
            lock (_cache)
            {
                return _cache.Dequeue();
            }
        }

        public void Enqueue(T instance)
        {
            lock (_cache)
            {
                _cache.Enqueue(instance);
            }
        }
    }
}