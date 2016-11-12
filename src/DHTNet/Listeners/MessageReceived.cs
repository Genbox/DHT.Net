using System.Net;

namespace DHTNet.Listeners
{
    public delegate void MessageReceived(byte[] buffer, IPEndPoint endpoint);
}