//
// QueryMessage.cs
//
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
//


using System;
using DHTNet.BEncode;
using DHTNet.Nodes;

namespace DHTNet.Messages.Queries
{
    internal abstract class QueryMessage : Message
    {
        private static readonly BEncodedString _queryArgumentsKey = "a";
        private static readonly BEncodedString _queryNameKey = "q";
        internal static readonly BEncodedString QueryType = "q";

        protected QueryMessage(NodeId id, BEncodedString queryName, Func<BEncodedDictionary, QueryMessage, Message> responseCreator)
            : this(id, queryName, new BEncodedDictionary(), responseCreator)
        {
        }

        protected QueryMessage(NodeId id, BEncodedString queryName, BEncodedDictionary queryArguments, Func<BEncodedDictionary, QueryMessage, Message> responseCreator)
            : base(QueryType)
        {
            Properties.Add(_queryNameKey, queryName);
            Properties.Add(_queryArgumentsKey, queryArguments);

            Parameters.Add(IdKey, id.BencodedString());
            ResponseCreator = responseCreator;
        }

        protected QueryMessage(BEncodedDictionary d, Func<BEncodedDictionary, QueryMessage, Message> responseCreator)
            : base(d)
        {
            ResponseCreator = responseCreator;
        }

        internal override NodeId Id => new NodeId((BEncodedString) Parameters[IdKey]);

        internal Func<BEncodedDictionary, QueryMessage, Message> ResponseCreator { get; private set; }

        protected BEncodedDictionary Parameters => (BEncodedDictionary) Properties[_queryArgumentsKey];
    }
}