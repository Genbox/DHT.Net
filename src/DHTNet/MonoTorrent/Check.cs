using System;

namespace DHTNet.MonoTorrent
{
    public static class Check
    {
        private static void DoCheck(object toCheck, string name)
        {
            if (toCheck == null)
                throw new ArgumentNullException(name);
        }

        internal static void Cache(object cache)
        {
            DoCheck(cache, "cache");
        }

        public static void InfoHash(object infoHash)
        {
            DoCheck(infoHash, "infoHash");
        }

        public static void MagnetLink(object magnetLink)
        {
            DoCheck(magnetLink, "magnetLink");
        }
    }
}