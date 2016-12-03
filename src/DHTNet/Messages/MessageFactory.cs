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
using System.Collections.Concurrent;
using System.Collections.Generic;
using DHTNet.BEncode;
using DHTNet.Enums;
using DHTNet.Messages.Errors;
using DHTNet.Messages.Queries;

namespace DHTNet.Messages
{
    internal static class MessageFactory
    {
        private static readonly string _queryNameKey = "q";
        private static readonly BEncodedString _messageTypeKey = "y";
        private static readonly BEncodedString _transactionIdKey = "t";

        private static readonly ConcurrentDictionary<BEncodedValue, QueryMessage> _messages = new ConcurrentDictionary<BEncodedValue, QueryMessage>();
        private static readonly ConcurrentDictionary<BEncodedString, Func<BEncodedDictionary, DhtMessage>> _queryDecoders = new ConcurrentDictionary<BEncodedString, Func<BEncodedDictionary, DhtMessage>>();

        static MessageFactory()
        {
            _queryDecoders.TryAdd("announce_peer", d => new AnnouncePeer(d));
            _queryDecoders.TryAdd("find_node", d => new FindNode(d));
            _queryDecoders.TryAdd("get_peers", d => new GetPeers(d));
            _queryDecoders.TryAdd("ping", d => new Ping(d));
        }

        public static int RegisteredMessages => _messages.Count;

        internal static bool IsRegistered(BEncodedValue transactionId)
        {
            return _messages.ContainsKey(transactionId);
        }

        public static void RegisterSend(QueryMessage message)
        {
            _messages.TryAdd(message.TransactionId, message);
        }

        public static bool UnregisterSend(QueryMessage message)
        {
            QueryMessage notUsed;
            return _messages.TryRemove(message.TransactionId, out notUsed);
        }

        public static DhtMessage DecodeMessage(BEncodedDictionary dictionary)
        {
            DhtMessage message;
            string error;

            if (!TryDecodeMessage(dictionary, out message, out error))
                throw new DHTMessageException(ErrorCode.GenericError, error);

            return message;
        }

        public static bool TryDecodeMessage(BEncodedDictionary dictionary, out DhtMessage message, out string error)
        {
            message = null;
            error = null;

            if (dictionary[_messageTypeKey].Equals(QueryMessage.QueryType))
            {
                try
                {
                    message = _queryDecoders[(BEncodedString) dictionary[_queryNameKey]](dictionary);
                }
                catch (KeyNotFoundException)
                {
                    //DHT.NET: We catch unsupported RPCs here
                    error = "Unsupported RPC '" + (BEncodedString) dictionary[_queryNameKey] + "'";
                }
            }
            else if (dictionary[_messageTypeKey].Equals(ErrorMessage.ErrorType))
            {
                message = new ErrorMessage(dictionary);
            }
            else
            {
                QueryMessage query;
                BEncodedString key = (BEncodedString) dictionary[_transactionIdKey];
                if (_messages.TryGetValue(key, out query))
                {
                    QueryMessage notUsed;
                    _messages.TryRemove(key, out notUsed);

                    try
                    {
                        message = query.ResponseCreator(dictionary, query);
                    }
                    catch
                    {
                        error = "Response dictionary was invalid";
                    }
                }
                else
                {
                    error = "Response had bad transaction ID";
                }
            }

            return (error == null) && (message != null);
        }
    }
}