using System.Collections.Generic;

namespace DHTNet.MonoTorrent
{
    public class MonoTorrentCollection<T> : List<T>
    {
        public MonoTorrentCollection()
        {
        }

        public MonoTorrentCollection(IEnumerable<T> collection)
            : base(collection)
        {
        }

        public MonoTorrentCollection(int capacity)
            : base(capacity)
        {
        }

        public MonoTorrentCollection<T> Clone()
        {
            return new MonoTorrentCollection<T>(this);
        }

        public T Dequeue()
        {
            T result = this[0];
            RemoveAt(0);
            return result;
        }
    }
}