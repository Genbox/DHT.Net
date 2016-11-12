using System.Net;

namespace MonoTorrent.Dht.Listeners
{
    public delegate void MessageReceived(byte[] buffer, IPEndPoint endpoint);
}