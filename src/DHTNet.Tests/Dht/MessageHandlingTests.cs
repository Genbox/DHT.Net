using System;
using System.Net;
using System.Threading;
using DHTNet.BEncode;
using DHTNet.Enums;
using DHTNet.EventArgs;
using DHTNet.Messages.Queries;
using DHTNet.Messages.Responses;
using DHTNet.Nodes;
using DHTNet.Tasks;
using Xunit;

namespace DHTNet.Tests.Dht
{
    public class MessageHandlingTests : IDisposable
    {
        public  MessageHandlingTests()
        {
            _listener = new TestListener();
            _node = new Node(NodeId.Create(), new IPEndPoint(IPAddress.Any, 0));
            _engine = new DhtEngine(_listener);
        }

        private readonly BEncodedString _transactionId = "cc";
        private DhtEngine _engine;
        private Node _node;
        private TestListener _listener;

        [Fact]
        public void PingTimeout()
        {
            _engine.TimeOut = TimeSpan.FromHours(1);
            // Send ping
            Ping ping = new Ping(_node.Id);
            ping.TransactionId = _transactionId;

            ManualResetEvent handle = new ManualResetEvent(false);
            SendQueryTask task = new SendQueryTask(_engine, ping, _node);
            task.Completed += delegate { handle.Set(); };
            task.Execute();

            // Receive response
            PingResponse response = new PingResponse(_node.Id, _transactionId);
            _listener.RaiseMessageReceived(response, _node.EndPoint);

            Assert.True(handle.WaitOne(1000), "#0");

            _engine.TimeOut = TimeSpan.FromMilliseconds(75);
            DateTime lastSeen = _node.LastSeen;

            // Time out a ping
            ping = new Ping(_node.Id);
            ping.TransactionId = (BEncodedString) "ab";

            task = new SendQueryTask(_engine, ping, _node, 4);
            task.Completed += delegate { handle.Set(); };

            handle.Reset();
            task.Execute();
            handle.WaitOne();

            Assert.Equal(4, _node.FailedCount);
            Assert.Equal(NodeState.Bad, _node.State);
            Assert.Equal(lastSeen, _node.LastSeen);
        }

        [Fact]
        public void SendPing()
        {
            _engine.Add(_node);
            _engine.TimeOut = TimeSpan.FromMilliseconds(75);
            ManualResetEvent handle = new ManualResetEvent(false);
            _engine.MessageLoop.QuerySent += delegate(object o, SendQueryEventArgs e)
            {
                if (!e.TimedOut && e.Query is Ping)
                    handle.Set();

                if (!e.TimedOut || !(e.Query is Ping))
                    return;

                PingResponse response = new PingResponse(_node.Id, e.Query.TransactionId);
                _listener.RaiseMessageReceived(response, e.EndPoint);
            };

            Assert.Equal(NodeState.Unknown, _node.State);

            DateTime lastSeen = _node.LastSeen;
            Assert.True(handle.WaitOne(1000), "#1a");
            Node nnnn = _node;
            _node = _engine.RoutingTable.FindNode(nnnn.Id);
            Assert.True(lastSeen < _node.LastSeen);
            Assert.Equal(NodeState.Good, _node.State);
        }

        public void Dispose()
        {
            _engine.Dispose();
        }
    }
}