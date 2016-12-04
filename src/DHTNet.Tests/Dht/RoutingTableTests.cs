using System.Collections.Generic;
using System.Net;
using DHTNet.Nodes;
using DHTNet.RoutingTable;
using Xunit;

namespace DHTNet.Tests.Dht
{
    public class RoutingTableTests
    {
        public RoutingTableTests()
        {
            _id = new byte[20];
            _id[1] = 128;
            _n = new Node(new NodeId(_id), new IPEndPoint(IPAddress.Any, 0));
            _table = new RoutingTable.RoutingTable(_n);
            _table.NodeAdded += delegate { _addedCount++; };
            _table.Add(_n); //the local node is no more in routing table so add it to show test is still ok
            _addedCount = 0;
        }

        //static void Main(string[] args)
        //{
        //    RoutingTableTests t = new RoutingTableTests();
        //    t.Setup();
        //    t.AddSame();
        //    t.Setup();
        //    t.AddSimilar();
        //}
        private byte[] _id;
        private RoutingTable.RoutingTable _table;
        private Node _n;
        private int _addedCount;

        private void CheckBuckets()
        {
            foreach (Bucket b in _table.Buckets)
                foreach (Node n in b.Nodes)
                    Assert.True((n.Id >= b.Min) && (n.Id < b.Max));
        }

        [Fact]
        public void AddSame()
        {
            _table.Clear();
            for (int i = 0; i < Config.MaxBucketCapacity; i++)
            {
                byte[] id = (byte[]) _id.Clone();
                _table.Add(new Node(new NodeId(id), new IPEndPoint(IPAddress.Any, 0)));
            }

            Assert.Equal(1, _addedCount);
            Assert.Equal(1, _table.Buckets.Count);
            Assert.Equal(1, _table.Buckets[0].Nodes.Count);

            CheckBuckets();
        }

        [Fact]
        public void AddSimilar()
        {
            for (int i = 0; i < Config.MaxBucketCapacity * 3; i++)
            {
                byte[] id = (byte[]) _id.Clone();
                id[0] += (byte) i;
                _table.Add(new Node(new NodeId(id), new IPEndPoint(IPAddress.Any, 0)));
            }

            Assert.Equal(Config.MaxBucketCapacity * 3 - 1, _addedCount);
            Assert.Equal(6, _table.Buckets.Count);
            Assert.Equal(8, _table.Buckets[0].Nodes.Count);
            Assert.Equal(8, _table.Buckets[1].Nodes.Count);
            Assert.Equal(8, _table.Buckets[2].Nodes.Count);
            Assert.Equal(0, _table.Buckets[3].Nodes.Count);
            Assert.Equal(0, _table.Buckets[4].Nodes.Count);
            Assert.Equal(0, _table.Buckets[5].Nodes.Count);
            CheckBuckets();
        }

        [Fact]
        public void GetClosestTest()
        {
            List<NodeId> nodes;
            TestHelper.ManyNodes(out _table, out nodes);


            List<Node> closest = _table.GetClosest(_table.LocalNode.Id);
            Assert.Equal(8, closest.Count);
            for (int i = 0; i < 8; i++)
                Assert.True(closest.Exists(delegate { return nodes[i].Equals(closest[i].Id); }));
        }
    }
}