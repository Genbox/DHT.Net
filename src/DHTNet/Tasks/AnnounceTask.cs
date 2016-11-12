using DHTNet.EventArgs;
using DHTNet.Messages.Queries;
using DHTNet.MonoTorrent;
using DHTNet.Nodes;

namespace DHTNet.Tasks
{
    internal class AnnounceTask : Task
    {
        private int _activeAnnounces;
        private readonly DhtEngine _engine;
        private readonly NodeId _infoHash;
        private readonly int _port;

        public AnnounceTask(DhtEngine engine, InfoHash infoHash, int port)
            : this(engine, new NodeId(infoHash), port)
        {
        }

        public AnnounceTask(DhtEngine engine, NodeId infoHash, int port)
        {
            _engine = engine;
            _infoHash = infoHash;
            _port = port;
        }

        public override void Execute()
        {
            GetPeersTask task = new GetPeersTask(_engine, _infoHash);
            task.Completed += GotPeers;
            task.Execute();
        }

        private void GotPeers(object o, TaskCompleteEventArgs e)
        {
            e.Task.Completed -= GotPeers;
            GetPeersTask getpeers = (GetPeersTask) e.Task;
            foreach (Node n in getpeers.ClosestActiveNodes.Values)
            {
                if (n.Token == null)
                    continue;
                AnnouncePeer query = new AnnouncePeer(_engine.LocalId, _infoHash, _port, n.Token);
                SendQueryTask task = new SendQueryTask(_engine, query, n);
                task.Completed += SentAnnounce;
                task.Execute();
                _activeAnnounces++;
            }

            if (_activeAnnounces == 0)
                RaiseComplete(new TaskCompleteEventArgs(this));
        }

        private void SentAnnounce(object o, TaskCompleteEventArgs e)
        {
            e.Task.Completed -= SentAnnounce;
            _activeAnnounces--;

            if (_activeAnnounces == 0)
                RaiseComplete(new TaskCompleteEventArgs(this));
        }
    }
}