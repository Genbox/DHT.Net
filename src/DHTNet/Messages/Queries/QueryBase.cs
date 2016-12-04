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
using DHTNet.Nodes;

namespace DHTNet.Messages.Queries
{
    internal abstract class QueryBase : DhtMessage
    {
        private static readonly BEncodedString _queryArgumentsKey = "a";
        private static readonly BEncodedString _queryNameKey = "q";
        internal static readonly BEncodedString QueryType = "q";

        protected QueryBase(NodeId id, BEncodedString queryName, Func<BEncodedDictionary, QueryBase, DhtMessage> responseCreator)
            : this(id, queryName, new BEncodedDictionary(), responseCreator)
        {
        }

        protected QueryBase(NodeId id, BEncodedString queryName, BEncodedDictionary queryArguments, Func<BEncodedDictionary, QueryBase, DhtMessage> responseCreator)
            : base(QueryType)
        {
            Properties.Add(_queryNameKey, queryName);
            Properties.Add(_queryArgumentsKey, queryArguments);

            Arguments.Add(IdKey, id.BencodedString());
            ResponseCreator = responseCreator;
        }

        protected QueryBase(BEncodedDictionary d, Func<BEncodedDictionary, QueryBase, DhtMessage> responseCreator)
            : base(d)
        {
            ResponseCreator = responseCreator;
        }

        internal override NodeId Id => new NodeId(((BEncodedString)Arguments[IdKey]).TextBytes);

        internal Func<BEncodedDictionary, QueryBase, DhtMessage> ResponseCreator { get; private set; }

        protected BEncodedDictionary Arguments => (BEncodedDictionary)Properties[_queryArgumentsKey];
    }
}