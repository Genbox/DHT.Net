namespace MonoTorrent.Common
{
    interface ICache<T>
    {
        int Count { get; }
        T Dequeue();
        void Enqueue (T instance);
    }
}