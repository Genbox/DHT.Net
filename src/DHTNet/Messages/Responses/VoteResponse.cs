using DHTNet.BEncode;
using DHTNet.Messages.Queries;
using DHTNet.Nodes;

namespace DHTNet.Messages.Responses
{
    internal class VoteResponse : ResponseBase
    {
        private static readonly BEncodedString _voteKey = "vote";
        private static readonly BEncodedString _tokenKey = "token";

        public VoteResponse(NodeId id, BEncodedValue transactionId, BEncodedString token)
            : base(id, transactionId)
        {
            ReturnValues.Add(_tokenKey, token);
        }

        public VoteResponse(BEncodedDictionary d, QueryBase m)
            : base(d, m)
        {
        }

        public BEncodedString Token
        {
            get { return (BEncodedString)ReturnValues[_tokenKey]; }
            set { ReturnValues[_tokenKey] = value; }
        }

        public BEncodedNumber Vote
        {
            get
            {
                if (!ReturnValues.ContainsKey(_voteKey))
                    return null;

                return (BEncodedNumber)ReturnValues[_voteKey];
            }
            set
            {
                ReturnValues[_voteKey] = value;
            }
        }

        public override void Handle(DhtEngine engine, Node node)
        {
            base.Handle(engine, node);
            node.Token = Token;
        }
    }
}