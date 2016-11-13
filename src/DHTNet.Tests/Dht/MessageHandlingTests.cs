using System;
using System.Net;
using System.Threading;
using DHTNet.BEncode;
using DHTNet.EventArgs;
using DHTNet.Messages.Queries;
using DHTNet.Messages.Responses;
using DHTNet.Nodes;
using DHTNet.Tasks;
using NUnit.Framework;

namespace DHTNet.Tests.Dht
{
    [TestFixture]
    public class MessageHandlingTests
    {
        [SetUp]
        public void Setup()
        {
            _listener = new TestListener();
            _node = new Node(NodeId.Create(), new IPEndPoint(IPAddress.Any, 0));
            _engine = new DhtEngine(_listener);
            //engine.Add(node);
        }

        [TearDown]
        public void Teardown()
        {
            _engine.Dispose();
        }

        private readonly BEncodedString _transactionId = "cc";
        private DhtEngine _engine;
        private Node _node;
        private TestListener _listener;

        [Test]
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

            Assert.IsTrue(handle.WaitOne(1000), "#0");

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

            Assert.AreEqual(4, _node.FailedCount, "#1");
            Assert.AreEqual(NodeState.Bad, _node.State, "#2");
            Assert.AreEqual(lastSeen, _node.LastSeen, "#3");
        }

        [Test]
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

            Assert.AreEqual(NodeState.Unknown, _node.State, "#1");

            DateTime lastSeen = _node.LastSeen;
            Assert.IsTrue(handle.WaitOne(1000), "#1a");
            Node nnnn = _node;
            _node = _engine.RoutingTable.FindNode(nnnn.Id);
            Assert.IsTrue(lastSeen < _node.LastSeen, "#2");
            Assert.AreEqual(NodeState.Good, _node.State, "#3");
        }
    }
}