using System;
using System.Text;
using DHTNet.BEncode;
using DHTNet.Messages;
using DHTNet.Messages.Queries;
using DHTNet.Messages.Responses;
using DHTNet.Nodes;
using Xunit;

namespace DHTNet.Tests.Dht
{
    public class MessageTests : IDisposable
    {
        public MessageTests()
        {
            DhtMessage.UseVersionKey = false;
        }

        private readonly NodeId _id = new NodeId(Encoding.UTF8.GetBytes("abcdefghij0123456789"));
        private readonly NodeId _infohash = new NodeId(Encoding.UTF8.GetBytes("mnopqrstuvwxyz123456"));
        private readonly BEncodedString _token = "aoeusnth";
        private readonly BEncodedString _transactionId = "aa";

        private QueryBase _message;


        private void Compare(Message m, string expected)
        {
            byte[] b = m.EncodeBytes();
            Assert.Equal(Encoding.UTF8.GetString(b), expected);
        }

        private Message Decode(string p)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(p);
            return MessageFactory.DecodeMessage(BEncodedValue.Decode<BEncodedDictionary>(buffer));
        }

        [Fact]
        public void AnnouncePeerDecode()
        {
            string text = "d1:ad2:id20:abcdefghij01234567899:info_hash20:mnopqrstuvwxyz1234564:porti6881e5:token8:aoeusnthe1:q13:announce_peer1:t2:aa1:y1:qe";
            AnnouncePeer m = (AnnouncePeer) Decode("d1:ad2:id20:abcdefghij01234567899:info_hash20:mnopqrstuvwxyz1234564:porti6881e5:token8:aoeusnthe1:q13:announce_peer1:t2:aa1:y1:qe");
            Assert.Equal(m.TransactionId, _transactionId);
            Assert.Equal(m.MessageType, QueryBase.QueryType);
            Assert.Equal(_id, m.Id);
            Assert.Equal(_infohash, m.InfoHash);
            Assert.Equal((BEncodedNumber) 6881, m.Port);
            Assert.Equal(_token, m.Token);

            Compare(m, text);
            _message = m;
        }

        [Fact]
        public void AnnouncePeerEncode()
        {
            Node n = new Node(NodeId.Create(), null);
            n.Token = _token;
            AnnouncePeer m = new AnnouncePeer(_id, _infohash, 6881, _token);
            m.TransactionId = _transactionId;

            Compare(m, "d1:ad2:id20:abcdefghij01234567899:info_hash20:mnopqrstuvwxyz1234564:porti6881e5:token8:aoeusnthe1:q13:announce_peer1:t2:aa1:y1:qe");
        }


        [Fact]
        public void AnnouncePeerResponseDecode()
        {
            // Register the query as being sent so we can decode the response
            AnnouncePeerDecode();
            MessageFactory.RegisterSend(_message);
            string text = "d1:rd2:id20:mnopqrstuvwxyz123456e1:t2:aa1:y1:re";

            AnnouncePeerResponse m = (AnnouncePeerResponse) Decode(text);
            Assert.Equal(_infohash, m.Id);

            Compare(m, text);
        }

        [Fact]
        public void AnnouncePeerResponseEncode()
        {
            AnnouncePeerResponse m = new AnnouncePeerResponse(_infohash, _transactionId);

            Compare(m, "d1:rd2:id20:mnopqrstuvwxyz123456e1:t2:aa1:y1:re");
        }

        [Fact]
        public void FindNodeDecode()
        {
            string text = "d1:ad2:id20:abcdefghij01234567896:target20:mnopqrstuvwxyz123456e1:q9:find_node1:t2:aa1:y1:qe";
            FindNode m = (FindNode) Decode(text);

            Assert.Equal(_id, m.Id);
            Assert.Equal(_infohash, m.Target);
            Compare(m, text);
        }

        [Fact]
        public void FindNodeEncode()
        {
            FindNode m = new FindNode(_id, _infohash);
            m.TransactionId = _transactionId;

            Compare(m, "d1:ad2:id20:abcdefghij01234567896:target20:mnopqrstuvwxyz123456e1:q9:find_node1:t2:aa1:y1:qe");
            _message = m;
        }

        [Fact]
        public void FindNodeResponseDecode()
        {
            FindNodeEncode();
            MessageFactory.RegisterSend(_message);
            string text = "d1:rd2:id20:abcdefghij01234567895:nodes9:def456...e1:t2:aa1:y1:re";
            FindNodeResponse m = (FindNodeResponse) Decode(text);

            Assert.Equal(_id, m.Id);
            Assert.Equal((BEncodedString) "def456...", m.Nodes);
            Assert.Equal(_transactionId, m.TransactionId);

            Compare(m, text);
        }

        [Fact]
        public void FindNodeResponseEncode()
        {
            FindNodeResponse m = new FindNodeResponse(_id, _transactionId);
            m.Nodes = "def456...";

            Compare(m, "d1:rd2:id20:abcdefghij01234567895:nodes9:def456...e1:t2:aa1:y1:re");
        }

        [Fact]
        public void GetPeersDecode()
        {
            string text = "d1:ad2:id20:abcdefghij01234567899:info_hash20:mnopqrstuvwxyz123456e1:q9:get_peers1:t2:aa1:y1:qe";
            GetPeers m = (GetPeers) Decode(text);

            Assert.Equal(_infohash, m.InfoHash);
            Assert.Equal(_id, m.Id);
            Assert.Equal(_transactionId, m.TransactionId);

            Compare(m, text);
        }

        [Fact]
        public void GetPeersEncode()
        {
            GetPeers m = new GetPeers(_id, _infohash);
            m.TransactionId = _transactionId;

            Compare(m, "d1:ad2:id20:abcdefghij01234567899:info_hash20:mnopqrstuvwxyz123456e1:q9:get_peers1:t2:aa1:y1:qe");
            _message = m;
        }

        [Fact]
        public void GetPeersResponseDecode()
        {
            GetPeersEncode();
            MessageFactory.RegisterSend(_message);

            string text = "d1:rd2:id20:abcdefghij01234567895:token8:aoeusnth6:valuesl6:axje.u6:idhtnmee1:t2:aa1:y1:re";
            GetPeersResponse m = (GetPeersResponse) Decode(text);

            Assert.Equal(_token, m.Token);
            Assert.Equal(_id, m.Id);

            BEncodedList l = new BEncodedList();
            l.Add((BEncodedString) "axje.u");
            l.Add((BEncodedString) "idhtnm");
            Assert.Equal(l, m.Values);

            Compare(m, text);
        }

        [Fact]
        public void GetPeersResponseEncode()
        {
            GetPeersResponse m = new GetPeersResponse(_id, _transactionId, _token);
            m.Values = new BEncodedList();
            m.Values.Add((BEncodedString) "axje.u");
            m.Values.Add((BEncodedString) "idhtnm");
            Compare(m, "d1:rd2:id20:abcdefghij01234567895:token8:aoeusnth6:valuesl6:axje.u6:idhtnmee1:t2:aa1:y1:re");
        }

        [Fact]
        public void PingDecode()
        {
            string text = "d1:ad2:id20:abcdefghij0123456789e1:q4:ping1:t2:aa1:y1:qe";
            Ping m = (Ping) Decode(text);

            Assert.Equal(_id, m.Id);

            Compare(m, text);
        }

        [Fact]
        public void PingEncode()
        {
            Ping m = new Ping(_id);
            m.TransactionId = _transactionId;

            Compare(m, "d1:ad2:id20:abcdefghij0123456789e1:q4:ping1:t2:aa1:y1:qe");
            _message = m;
        }

        [Fact]
        public void PingResponseDecode()
        {
            PingEncode();
            MessageFactory.RegisterSend(_message);

            string text = "d1:rd2:id20:mnopqrstuvwxyz123456e1:t2:aa1:y1:re";
            PingResponse m = (PingResponse) Decode(text);

            Assert.Equal(_infohash, m.Id);

            Compare(m, "d1:rd2:id20:mnopqrstuvwxyz123456e1:t2:aa1:y1:re");
        }

        [Fact]
        public void PingResponseEncode()
        {
            PingResponse m = new PingResponse(_infohash, _transactionId);

            Compare(m, "d1:rd2:id20:mnopqrstuvwxyz123456e1:t2:aa1:y1:re");
        }

        public void Dispose()
        {
            DhtMessage.UseVersionKey = true;
        }
    }
}