using System;
using System.Net;
using DHTNet.Listeners;
using DHTNet.MonoTorrent;

namespace DHTNet.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DhtListener listener = new DhtListener(new IPEndPoint(IPAddress.Any, 15000));
            listener.Start();

            DhtEngine engine = new DhtEngine(listener);
            engine.PeersFound += EngineOnPeersFound;
            engine.Start();

            Console.ReadLine();
        }

        private static void EngineOnPeersFound(object sender, PeersFoundEventArgs peersFoundEventArgs)
        {
            Console.WriteLine("Peer found");
        }
    }
}
