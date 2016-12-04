using System;
using DHTNet.BEncode;
using DHTNet.Messages.Responses;
using DHTNet.Nodes;

namespace DHTNet.Messages.Queries
{
    internal class Vote : QueryBase
    {
        private static readonly BEncodedString _targetKey = "target";
        private static readonly BEncodedString _voteKey = "vote";
        private static readonly Func<BEncodedDictionary, QueryBase, DhtMessage> _responseCreator = (d, m) => new GetPeersResponse(d, m);

        public Vote(NodeId id, NodeId target, byte vote)
            : base(id, _voteKey, _responseCreator)
        {
            Arguments.Add(_targetKey, target.BencodedString());
            Arguments.Add(_voteKey, new BEncodedNumber(vote));
        }

        public Vote(BEncodedDictionary d)
            : base(d, _responseCreator)
        {
        }

        public NodeId Target => new NodeId(((BEncodedString)Arguments[_targetKey]).TextBytes);

        public override void Handle(DhtEngine engine, Node node)
        {
            base.Handle(engine, node);

            BEncodedString token = engine.TokenManager.GenerateToken(node);
            VoteResponse response = new VoteResponse(engine.RoutingTable.LocalNode.Id, TransactionId, token);

            engine.MessageLoop.EnqueueSend(response, node.EndPoint);
        }
    }
}