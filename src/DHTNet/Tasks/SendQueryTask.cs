using System;
using DHTNet.EventArgs;
using DHTNet.Messages.Queries;
using DHTNet.Nodes;

namespace DHTNet.Tasks
{
    internal class SendQueryTask : Task
    {
        private readonly DhtEngine _engine;
        private readonly QueryMessage _query;
        private int _retries;

        public SendQueryTask(DhtEngine engine, QueryMessage query, Node node)
            : this(engine, query, node, 3)
        {
        }

        public SendQueryTask(DhtEngine engine, QueryMessage query, Node node, int retries)
        {
            if (engine == null)
                throw new ArgumentNullException(nameof(engine));
            if (query == null)
                throw new ArgumentNullException("message");
            if (node == null)
                throw new ArgumentNullException("message");

            _engine = engine;
            _query = query;
            Target = node;
            _retries = retries;
            Retries = retries;
        }

        public int Retries { get; }

        public Node Target { get; }

        public override void Execute()
        {
            if (Active)
                return;
            Hook();
            _engine.MessageLoop.EnqueueSend(_query, Target);
        }

        private void Hook()
        {
            _engine.MessageLoop.QuerySent += MessageSent;
        }

        private void MessageSent(object sender, SendQueryEventArgs e)
        {
            if (e.Query != _query)
                return;

            // If the message timed out and we we haven't already hit the maximum retries
            // send again. Otherwise we propagate the eventargs through the Complete event.
            if (e.TimedOut)
                Target.FailedCount++;
            else
                Target.LastSeen = DateTime.UtcNow;

            if (e.TimedOut && (--_retries > 0))
                _engine.MessageLoop.EnqueueSend(_query, Target);
            else
                RaiseComplete(e);
        }

        protected override void RaiseComplete(TaskCompleteEventArgs e)
        {
            Unhook();
            e.Task = this;
            base.RaiseComplete(e);
        }

        private void Unhook()
        {
            _engine.MessageLoop.QuerySent -= MessageSent;
        }
    }
}