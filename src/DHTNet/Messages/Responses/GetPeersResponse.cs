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
using DHTNet.Messages.Queries;
using DHTNet.Nodes;

namespace DHTNet.Messages.Responses
{
    internal class GetPeersResponse : ResponseBase
    {
        private static readonly BEncodedString _nodesKey = "nodes";
        private static readonly BEncodedString _tokenKey = "token";
        private static readonly BEncodedString _valuesKey = "values";

        public GetPeersResponse(NodeId id, BEncodedValue transactionId, BEncodedString token)
            : base(id, transactionId)
        {
            ReturnValues.Add(_tokenKey, token);
        }

        public GetPeersResponse(BEncodedDictionary d, QueryBase m)
            : base(d, m)
        {
        }

        public BEncodedString Token
        {
            get { return (BEncodedString) ReturnValues[_tokenKey]; }
            set { ReturnValues[_tokenKey] = value; }
        }

        public BEncodedString Nodes
        {
            get
            {
                if (ReturnValues.ContainsKey(_valuesKey) || !ReturnValues.ContainsKey(_nodesKey))
                    return null;
                return (BEncodedString) ReturnValues[_nodesKey];
            }
            set
            {
                if (ReturnValues.ContainsKey(_valuesKey))
                    throw new InvalidOperationException("Already contains the values key");
                if (!ReturnValues.ContainsKey(_nodesKey))
                    ReturnValues.Add(_nodesKey, null);
                ReturnValues[_nodesKey] = value;
            }
        }

        public BEncodedList Values
        {
            get
            {
                if (ReturnValues.ContainsKey(_nodesKey) || !ReturnValues.ContainsKey(_valuesKey))
                    return null;
                return (BEncodedList) ReturnValues[_valuesKey];
            }
            set
            {
                if (ReturnValues.ContainsKey(_nodesKey))
                    throw new InvalidOperationException("Already contains the nodes key");
                if (!ReturnValues.ContainsKey(_valuesKey))
                    ReturnValues.Add(_valuesKey, value);
                else
                    ReturnValues[_valuesKey] = value;
            }
        }

        public override void Handle(DhtEngine engine, Node node)
        {
            base.Handle(engine, node);
            node.Token = Token;
            if (Nodes != null)
                engine.Add(Node.FromCompactNode(Nodes));
        }
    }
}