using DHTNet.Nodes;
using Xunit;

namespace DHTNet.Tests.Dht
{
    public class NodeIdTests
    {
        public NodeIdTests()
        {
            _nodes = new NodeId[20];
            for (int i = 0; i < _nodes.Length; i++)
            {
                byte[] id = new byte[20];
                for (int j = 0; j < id.Length; j++)
                    id[j] = (byte) (i * 20 + j);
                _nodes[i] = new NodeId(id);
            }
        }

        private NodeId[] _nodes;

        [Fact]
        public void CompareTest()
        {
            byte[] i = new byte[20];
            byte[] j = new byte[20];
            i[19] = 1;
            j[19] = 2;
            NodeId one = new NodeId(i);
            NodeId two = new NodeId(j);
            Assert.True(one.CompareTo(two) < 0);
            Assert.True(two.CompareTo(one) > 0);
            Assert.True(one.CompareTo(one) == 0);
        }

        [Fact]
        public void CompareTest2()
        {
            byte[] data = {1, 179, 114, 132, 233, 117, 195, 250, 164, 35, 157, 48, 170, 96, 87, 111, 42, 137, 195, 199};
            BigInteger a = new BigInteger(data);
            BigInteger b = new BigInteger(new byte[0]);

            Assert.NotEqual(a, b);
        }

        [Fact]
        public void GreaterLessThanTest()
        {
            Assert.True(_nodes[0] < _nodes[1]);
            Assert.True(_nodes[1] > _nodes[0]);
            Assert.True(_nodes[0] == _nodes[0]);
            Assert.Equal(_nodes[0], _nodes[0]);
            Assert.True(_nodes[2] > _nodes[1]);
            Assert.True(_nodes[15] < _nodes[10], "#6");
        }

        [Fact]
        public void XorTest()
        {
            NodeId zero = new NodeId(new byte[20]);

            byte[] b = new byte[20];
            b[0] = 1;
            NodeId one = new NodeId(b);

            NodeId r = one.Xor(zero);
            Assert.Equal(one, r);
            Assert.True(one > zero);
            Assert.True(one.CompareTo(zero) > 0);

            NodeId z = one.Xor(r);
            Assert.Equal(zero, z);
        }
    }
}