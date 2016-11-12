#if !DISABLE_DHT
using DHTNet.Nodes;

namespace DHTNet.EventArgs
{
    internal class NodeAddedEventArgs : System.EventArgs
    {
        private Node node;

        public Node Node
        {
            get { return node; }
        }

        public NodeAddedEventArgs(Node node)
        {
            this.node = node;
        }
    }
}
#endif