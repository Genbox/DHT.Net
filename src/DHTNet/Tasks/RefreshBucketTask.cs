using System;
using DHTNet.EventArgs;
using DHTNet.Messages.Queries;
using DHTNet.Nodes;
using DHTNet.RoutingTable;

namespace DHTNet.Tasks
{
    internal class RefreshBucketTask : Task
    {
        private readonly Bucket _bucket;
        private readonly DhtEngine _engine;
        private FindNode _message;
        private Node _node;
        private SendQueryTask _task;

        public RefreshBucketTask(DhtEngine engine, Bucket bucket)
        {
            this._engine = engine;
            this._bucket = bucket;
        }

        public override void Execute()
        {
            if (_bucket.Nodes.Count == 0)
            {
                RaiseComplete(new TaskCompleteEventArgs(this));
                return;
            }

            Console.WriteLine("Choosing first from: {0}", _bucket.Nodes.Count);
            _bucket.SortBySeen();
            QueryNode(_bucket.Nodes[0]);
        }

        private void TaskComplete(object o, TaskCompleteEventArgs e)
        {
            _task.Completed -= TaskComplete;

            SendQueryEventArgs args = (SendQueryEventArgs) e;
            if (args.TimedOut)
            {
                _bucket.SortBySeen();
                int index = _bucket.Nodes.IndexOf(_node);
                if ((index == -1) || (++index < _bucket.Nodes.Count))
                    QueryNode(_bucket.Nodes[0]);
                else
                    RaiseComplete(new TaskCompleteEventArgs(this));
            }
            else
            {
                RaiseComplete(new TaskCompleteEventArgs(this));
            }
        }

        private void QueryNode(Node node)
        {
            this._node = node;
            _message = new FindNode(_engine.LocalId, node.Id);
            _task = new SendQueryTask(_engine, _message, node);
            _task.Completed += TaskComplete;
            _task.Execute();
        }
    }
}