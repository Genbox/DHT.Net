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
    /// Find node is used to find the contact information for a node given its ID. "q" == "find_node".
    /// A find_node query has two arguments, "id" containing the node ID of the querying node, and "target" containing the ID of the node sought by the queryer.
    /// When a node receives a find_node query, it should respond with a key "nodes" and value of a string containing the compact
    /// node info for the target node or the K (8) closest good nodes in its own routing table.
    /// </summary>
    internal class FindNode : QueryMessage
    {
        private static readonly BEncodedString _targetKey = "target";
        private static readonly BEncodedString _queryName = "find_node";
        private static readonly Func<BEncodedDictionary, QueryMessage, DhtMessage> _responseCreator = (d, m) => new FindNodeResponse(d, m);

        public FindNode(NodeId id, NodeId target)
            : base(id, _queryName, _responseCreator)
        {
            Parameters.Add(_targetKey, target.BencodedString());
        }

        public FindNode(BEncodedDictionary d)
            : base(d, _responseCreator)
        {
        }

        public NodeId Target => new NodeId(((BEncodedString) Parameters[_targetKey]).TextBytes);

        public override void Handle(DhtEngine engine, Node node)
        {
            base.Handle(engine, node);

            FindNodeResponse response = new FindNodeResponse(engine.RoutingTable.LocalNode.Id, TransactionId);

            Node targetNode = engine.RoutingTable.FindNode(Target);
            if (targetNode != null)
                response.Nodes = targetNode.CompactNode();
            else
                response.Nodes = Node.CompactNode(engine.RoutingTable.GetClosest(Target));

            engine.MessageLoop.EnqueueSend(response, node.EndPoint);
        }
    }
}