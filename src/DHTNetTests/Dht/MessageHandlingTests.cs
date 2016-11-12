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
            listener = new TestListener();
            node = new Node(NodeId.Create(), new IPEndPoint(IPAddress.Any, 0));
            engine = new DhtEngine(listener);
            //engine.Add(node);
        }

        [TearDown]
        public void Teardown()
        {
            engine.Dispose();
        }

        //static void Main(string[] args)
        //{
        //    TaskTests t = new TaskTests();
        //    t.Setup();
        //    t.BucketRefreshTest();
        //}
        private readonly BEncodedString transactionId = "cc";
        private DhtEngine engine;
        private Node node;
        private TestListener listener;

        [Test]
        public void PingTimeout()
        {
            engine.TimeOut = TimeSpan.FromHours(1);
            // Send ping
            Ping ping = new Ping(node.Id);
            ping.TransactionId = transactionId;

            ManualResetEvent handle = new ManualResetEvent(false);
            SendQueryTask task = new SendQueryTask(engine, ping, node);
            task.Completed += delegate { handle.Set(); };
            task.Execute();

            // Receive response
            PingResponse response = new PingResponse(node.Id, transactionId);
            listener.RaiseMessageReceived(response, node.EndPoint);

            Assert.IsTrue(handle.WaitOne(1000), "#0");

            engine.TimeOut = TimeSpan.FromMilliseconds(75);
            DateTime lastSeen = node.LastSeen;

            // Time out a ping
            ping = new Ping(node.Id);
            ping.TransactionId = (BEncodedString) "ab";

            task = new SendQueryTask(engine, ping, node, 4);
            task.Completed += delegate { handle.Set(); };

            handle.Reset();
            task.Execute();
            handle.WaitOne();

            Assert.AreEqual(4, node.FailedCount, "#1");
            Assert.AreEqual(NodeState.Bad, node.State, "#2");
            Assert.AreEqual(lastSeen, node.LastSeen, "#3");
        }

        [Test]
        public void SendPing()
        {
            engine.Add(node);
            engine.TimeOut = TimeSpan.FromMilliseconds(75);
            ManualResetEvent handle = new ManualResetEvent(false);
            engine.MessageLoop.QuerySent += delegate(object o, SendQueryEventArgs e)
            {
                if (!e.TimedOut && e.Query is Ping)
                    handle.Set();

                if (!e.TimedOut || !(e.Query is Ping))
                    return;

                PingResponse response = new PingResponse(node.Id, e.Query.TransactionId);
                listener.RaiseMessageReceived(response, e.EndPoint);
            };

            Assert.AreEqual(NodeState.Unknown, node.State, "#1");

            DateTime lastSeen = node.LastSeen;
            Assert.IsTrue(handle.WaitOne(1000), "#1a");
            Node nnnn = node;
            node = engine.RoutingTable.FindNode(nnnn.Id);
            Assert.IsTrue(lastSeen < node.LastSeen, "#2");
            Assert.AreEqual(NodeState.Good, node.State, "#3");
        }

//            listener.RaiseMessageReceived(response, task.Target.EndPoint);
//            PingResponse response = new PingResponse(task.Target.Id);
//            SendQueryTask task = (SendQueryTask)e.Task;
//
//                return;
//            if (!e.TimedOut || !(e.Query is Ping))
//        {

//        void FakePingResponse(object sender, SendQueryEventArgs e)
//        }
    }
}