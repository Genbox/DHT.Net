namespace DHTNet.MonoTorrent
{
    internal class SynchronizedCache<T> : ICache<T>
    {
        private readonly ICache<T> cache;

        public SynchronizedCache(ICache<T> cache)
        {
            Check.Cache(cache);
            this.cache = cache;
        }

        public int Count
        {
            get { return cache.Count; }
        }

        public T Dequeue()
        {
            lock (cache)
            {
                return cache.Dequeue();
            }
        }

        public void Enqueue(T instance)
        {
            lock (cache)
            {
                cache.Enqueue(instance);
            }
        }
    }
}