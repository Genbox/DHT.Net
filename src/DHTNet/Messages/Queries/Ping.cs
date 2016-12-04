// Authors:
//   Alan McGovern <alan.mcgovern@gmail.com>
//
// Copyright (C) 2008 Alan McGovern
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using DHTNet.BEncode;
using DHTNet.Messages.Responses;
using DHTNet.Nodes;

namespace DHTNet.Messages.Queries
{
    /// <summary>
    /// The most basic query is a ping. "q" = "ping" A ping query has a single argument, "id" the value is a 20-byte string containing the senders node ID in network byte order.
    /// The appropriate response to a ping has a single key "id" containing the node ID of the responding node.
    /// </summary>
    internal class Ping : QueryBase
    {
        private static readonly BEncodedString _queryName = "ping";
        private static readonly Func<BEncodedDictionary, QueryBase, DhtMessage> _responseCreator = (d, m) => new PingResponse(d, m);

        public Ping(NodeId id)
            : base(id, _queryName, _responseCreator)
        {
        }

        public Ping(BEncodedDictionary d)
            : base(d, _responseCreator)
        {
        }

        public override void Handle(DhtEngine engine, Node node)
        {
            base.Handle(engine, node);

            PingResponse m = new PingResponse(engine.RoutingTable.LocalNode.Id, TransactionId);
            engine.MessageLoop.EnqueueSend(m, node.EndPoint);
        }
    }
}