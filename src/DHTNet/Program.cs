using System;
using System.Net;
using System.Threading.Tasks;
using DHTNet.EventArgs;
using DHTNet.Listeners;
using DHTNet.Nodes;

namespace DHTNet
{
    public class Program
    {
        public static void Main()
        {
            //https://wiki.theory.org/BitTorrentSpecification
            Listener listener = new UdpListener(new IPEndPoint(IPAddress.Any, 15000));
            listener.Start();

            DhtEngine engine = new DhtEngine(listener);
            engine.Start();

            //Wait for the DHT to startup
            Task.Delay(30000).Wait();

            engine.PeersFound += EngineOnPeersFound;
            engine.GetPeers(InfoHash.FromHex("0403FB4728BD788FBCB67E87D6FEB241EF38C75A"));

            Console.ReadLine();
        }

        private static void EngineOnPeersFound(object sender, PeersFoundEventArgs peersFoundEventArgs)
        {
            throw new NotImplementedException();
        }
    }
}
