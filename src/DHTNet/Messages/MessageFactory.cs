//
// MessageFactory.cs
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

using System.Collections.Generic;
using DHTNet.BEncode;
using DHTNet.Messages.Errors;
using DHTNet.Messages.Queries;

namespace DHTNet.Messages
{
    internal static class MessageFactory
    {
        private static readonly string _queryNameKey = "q";
        private static readonly BEncodedString _messageTypeKey = "y";
        private static readonly BEncodedString _transactionIdKey = "t";

        private static readonly Dictionary<BEncodedValue, QueryMessage> _messages = new Dictionary<BEncodedValue, QueryMessage>();
        private static readonly Dictionary<BEncodedString, Creator> _queryDecoders = new Dictionary<BEncodedString, Creator>();

        static MessageFactory()
        {
            _queryDecoders.Add("announce_peer", d => new AnnouncePeer(d));
            _queryDecoders.Add("find_node", d => new FindNode(d));
            _queryDecoders.Add("get_peers", d => new GetPeers(d));
            _queryDecoders.Add("ping", d => new Ping(d));
        }

        public static int RegisteredMessages => _messages.Count;

        internal static bool IsRegistered(BEncodedValue transactionId)
        {
            return _messages.ContainsKey(transactionId);
        }

        public static void RegisterSend(QueryMessage message)
        {
            _messages.Add(message.TransactionId, message);
        }

        public static bool UnregisterSend(QueryMessage message)
        {
            return _messages.Remove(message.TransactionId);
        }

        public static Message DecodeMessage(BEncodedDictionary dictionary)
        {
            Message message;
            string error;

            if (!TryDecodeMessage(dictionary, out message, out error))
                throw new MessageException(ErrorCode.GenericError, error);

            return message;
        }

        public static bool TryDecodeMessage(BEncodedDictionary dictionary, out Message message)
        {
            string error;
            return TryDecodeMessage(dictionary, out message, out error);
        }

        public static bool TryDecodeMessage(BEncodedDictionary dictionary, out Message message, out string error)
        {
            message = null;
            error = null;

            if (dictionary[_messageTypeKey].Equals(QueryMessage.QueryType))
            {
                message = _queryDecoders[(BEncodedString) dictionary[_queryNameKey]](dictionary);
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
                    _messages.Remove(key);
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