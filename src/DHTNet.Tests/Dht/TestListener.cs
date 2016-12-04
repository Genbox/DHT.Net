using System;
using System.IO;
using System.Net;
using DHTNet.Listeners;
using DHTNet.Messages;

namespace DHTNet.Tests.Dht
{
    internal class TestListener : Listener
    {
        public TestListener()
            : base(new IPEndPoint(IPAddress.Loopback, 0))
        {
        }

        public bool Started { get; private set; }

        public override event Action<byte[], IPEndPoint> MessageReceived;


        public void RaiseMessageReceived(Message message, IPEndPoint endpoint)
        {
            DhtEngine.MainLoop.Queue(() => MessageReceived?.Invoke(message.EncodeBytes(), endpoint));
        }

        public override void Send(Stream stream, IPEndPoint endpoint)
        {
            // Do nothing
        }

        public override void Start()
        {
            Started = true;
        }

        public override void Stop()
        {
            Started = false;
        }
    }
}