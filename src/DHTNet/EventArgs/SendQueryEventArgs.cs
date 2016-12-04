using System.Net;
using DHTNet.Messages.Queries;
using DHTNet.Messages.Responses;

namespace DHTNet.EventArgs
{
    internal class SendQueryEventArgs : TaskCompleteEventArgs
    {
        public SendQueryEventArgs(IPEndPoint endpoint, QueryBase query, ResponseBase response)
            : base(null)
        {
            EndPoint = endpoint;
            Query = query;
            Response = response;
        }

        public IPEndPoint EndPoint { get; }

        public QueryBase Query { get; }

        public ResponseBase Response { get; }

        public bool TimedOut => Response == null;
    }
}