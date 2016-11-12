using System.Net;
using DHTNet.Messages.Queries;
using DHTNet.Messages.Responses;

namespace DHTNet.EventArgs
{
    internal class SendQueryEventArgs : TaskCompleteEventArgs
    {
        public SendQueryEventArgs(IPEndPoint endpoint, QueryMessage query, ResponseMessage response)
            : base(null)
        {
            EndPoint = endpoint;
            Query = query;
            Response = response;
        }

        public IPEndPoint EndPoint { get; }

        public QueryMessage Query { get; }

        public ResponseMessage Response { get; }

        public bool TimedOut
        {
            get { return Response == null; }
        }
    }
}