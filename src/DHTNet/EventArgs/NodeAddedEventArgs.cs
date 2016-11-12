using DHTNet.Nodes;

namespace DHTNet.EventArgs
{
    internal class NodeAddedEventArgs : System.EventArgs
    {
        public NodeAddedEventArgs(Node node)
        {
            Node = node;
        }

        public Node Node { get; }
    }
}