using System;
using System.Net;
using System.Threading;
using DHTNet.EventArgs;
using DHTNet.Listeners;
using DHTNet.Nodes;

namespace DHTNet.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Listener listener = new UdpListener(new IPEndPoint(IPAddress.Any, 15000));
            listener.Start();

            DhtEngine engine = new DhtEngine(listener);
            engine.PeersFound += EngineOnPeersFound;
            engine.Start();

            Thread.Sleep(25000);
            engine.GetPeers(InfoHash.FromHex("b415c913643e5ff49fe37d304bbb5e6e11ad5101"));

            Console.ReadLine();
        }

        private static void EngineOnPeersFound(object sender, PeersFoundEventArgs peersFoundEventArgs)
        {
            Console.WriteLine("Peer found: " + peersFoundEventArgs.Peers.Count);
        }
    }
}
