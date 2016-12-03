using System.Collections.Generic;
using DHTNet.EventArgs;
using DHTNet.Messages.Queries;
using DHTNet.Messages.Responses;
using DHTNet.Nodes;

namespace DHTNet.Tasks
{
    internal class GetPeersTask : Task
    {
        private readonly SortedList<NodeId, NodeId> _closestNodes;
        private readonly DhtEngine _engine;
        private readonly NodeId _infoHash;
        private int _activeQueries;

        public GetPeersTask(DhtEngine engine, InfoHash infohash)
            : this(engine, new NodeId(infohash))
        {
        }

        public GetPeersTask(DhtEngine engine, NodeId infohash)
        {
            _engine = engine;
            _infoHash = infohash;
            _closestNodes = new SortedList<NodeId, NodeId>(Config.MaxBucketCapacity);
            ClosestActiveNodes = new SortedList<NodeId, Node>(Config.MaxBucketCapacity * 2);
        }

        internal SortedList<NodeId, Node> ClosestActiveNodes { get; }

        public override void Execute()
        {
            if (Active)
                return;

            Active = true;
            DhtEngine.MainLoop.Queue(delegate
            {
                IEnumerable<Node> newNodes = _engine.RoutingTable.GetClosest(_infoHash);
                foreach (Node n in Node.CloserNodes(_infoHash, _closestNodes, newNodes, Config.MaxBucketCapacity))
                    SendGetPeers(n);
            });
        }

        private void SendGetPeers(Node n)
        {
            NodeId distance = n.Id.Xor(_infoHash);
            ClosestActiveNodes.Add(distance, n);

            _activeQueries++;
            GetPeers m = new GetPeers(_engine.LocalId, _infoHash);
            SendQueryTask task = new SendQueryTask(_engine, m, n);
            task.Completed += GetPeersCompleted;
            task.Execute();
        }

        private void GetPeersCompleted(object o, TaskCompleteEventArgs e)
        {
            try
            {
                _activeQueries--;
                e.Task.Completed -= GetPeersCompleted;

                SendQueryEventArgs args = (SendQueryEventArgs)e;

                // We want to keep a list of the top (K) closest nodes which have responded
                Node target = ((SendQueryTask)args.Task).Target;
                int index = ClosestActiveNodes.Values.IndexOf(target);
                if ((index >= Config.MaxBucketCapacity) || args.TimedOut)
                    ClosestActiveNodes.RemoveAt(index);

                if (args.TimedOut)
                    return;

                GetPeersResponse response = (GetPeersResponse)args.Response;

                // Ensure that the local Node object has the token. There may/may not be
                // an additional copy in the routing table depending on whether or not
                // it was able to fit into the table.
                target.Token = response.Token;
                if (response.Values != null)
                {
                    // We have actual peers!
                    _engine.RaisePeersFound(_infoHash, Peer.Decode(response.Values));
                }
                else if (response.Nodes != null)
                {
                    if (!Active)
                        return;

                    // We got a list of nodes which are closer
                    IEnumerable<Node> newNodes = Node.FromCompactNode(response.Nodes);
                    foreach (Node n in Node.CloserNodes(_infoHash, _closestNodes, newNodes, Config.MaxBucketCapacity))
                        SendGetPeers(n);
                }
            }
            finally
            {
                if (_activeQueries == 0)
                    RaiseComplete(new TaskCompleteEventArgs(this));
            }
        }

        protected override void RaiseComplete(TaskCompleteEventArgs e)
        {
            if (!Active)
                return;

            Active = false;
            base.RaiseComplete(e);
        }
    }
}