using System.Collections.Generic;
using System.Net;
using DHTNet.Nodes;

namespace DHTNet.Tests.Dht
{
    internal static class TestHelper
    {
        internal static void ManyNodes(out RoutingTable.RoutingTable routingTable, out List<NodeId> nodes)
        {
            // Generate our local id
            byte[] id = new byte[20];
            id[19] = 7;

            nodes = new List<NodeId>();
            RoutingTable.RoutingTable table = new RoutingTable.RoutingTable(new Node(new NodeId(id), new IPEndPoint(IPAddress.Any, 0)));

            for (int i = 0; i <= 30; i++)
            {
                if (i == 7)
                    continue;

                id = new byte[20];
                id[19] = (byte) i;
                nodes.Add(new NodeId(id));
                table.Add(new Node(new NodeId(id), new IPEndPoint(IPAddress.Any, 0)));
            }

            nodes.Sort(delegate(NodeId left, NodeId right)
            {
                NodeId dLeft = left.Xor(table.LocalNode.Id);
                NodeId dRight = right.Xor(table.LocalNode.Id);
                return dLeft.CompareTo(dRight);
            });

            nodes.RemoveAll(n => table.FindNode(n) == null);
            routingTable = table;
        }
    }
}