using System.Net;
using DHTNet.MonoTorrent;

namespace DHTNet.Listeners
{
    public class DhtListener : UdpListener
    {
        public DhtListener(IPEndPoint endpoint)
            : base(endpoint)
        {
        }

        public event MessageReceived MessageReceived;

        protected override void OnMessageReceived(byte[] buffer, IPEndPoint endpoint)
        {
            MessageReceived h = MessageReceived;
            if (h != null)
                h(buffer, endpoint);
        }
    }
}