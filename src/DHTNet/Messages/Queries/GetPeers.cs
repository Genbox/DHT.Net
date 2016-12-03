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
    ///Get peers associated with a torrent infohash.
    /// "q" = "get_peers" A get_peers query has two arguments, "id" containing the node ID of the querying node, and "info_hash" containing the infohash of the torrent.
    /// If the queried node has peers for the infohash, they are returned in a key "values" as a list of strings. Each string containing "compact" format peer information for a single peer.
    /// If the queried node has no peers for the infohash, a key "nodes" is returned containing the K nodes in the queried nodes routing table closest to the infohash supplied in the query.
    /// In either case a "token" key is also included in the return value. The token value is a required argument for a future announce_peer query.
    /// The token value should be a short binary string.
    /// </summary>
    internal class GetPeers : QueryMessage
    {
        private static readonly BEncodedString _infoHashKey = "info_hash";
        private static readonly BEncodedString _queryName = "get_peers";
        private static readonly Func<BEncodedDictionary, QueryMessage, DhtMessage> _responseCreator = (d, m) => new GetPeersResponse(d, m);

        public GetPeers(NodeId id, NodeId infohash)
            : base(id, _queryName, _responseCreator)
        {
            Parameters.Add(_infoHashKey, infohash.BencodedString());
        }

        public GetPeers(BEncodedDictionary d)
            : base(d, _responseCreator)
        {
        }

        public NodeId InfoHash => new NodeId(((BEncodedString) Parameters[_infoHashKey]).TextBytes);

        public override void Handle(DhtEngine engine, Node node)
        {
            base.Handle(engine, node);

            BEncodedString token = engine.TokenManager.GenerateToken(node);
            GetPeersResponse response = new GetPeersResponse(engine.RoutingTable.LocalNode.Id, TransactionId, token);
            if (engine.Torrents.ContainsKey(InfoHash))
            {
                BEncodedList list = new BEncodedList();
                foreach (Node n in engine.Torrents[InfoHash])
                    list.Add(n.CompactAddressPort());
                response.Values = list;
            }
            else
            {
                // Is this right?
                response.Nodes = Node.CompactNode(engine.RoutingTable.GetClosest(InfoHash));
            }

            engine.MessageLoop.EnqueueSend(response, node.EndPoint);
        }
    }
}