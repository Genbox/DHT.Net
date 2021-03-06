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
using System.IO;
using DHTNet.BEncode;
using DHTNet.Nodes;

namespace DHTNet.Messages
{
    /// <summary>
    /// This class is the base class for BEP 0005 DHT messages
    /// </summary>
    public abstract class DhtMessage : Message
    {
        internal static bool UseVersionKey = true;

        private static readonly BEncodedString _emptyString = "";
        protected static readonly BEncodedString IdKey = "id";
        private static readonly BEncodedString _transactionIdKey = "t";
        private static readonly BEncodedString _versionKey = "v";
        private static readonly BEncodedString _messageTypeKey = "y";
        private static readonly BEncodedString _dhtVersion = Config.DhtClientVersion;

        protected BEncodedDictionary Properties = new BEncodedDictionary();

        protected DhtMessage(BEncodedString messageType)
        {
            Properties.Add(_transactionIdKey, null);
            Properties.Add(_messageTypeKey, messageType);
            if (UseVersionKey)
                Properties.Add(_versionKey, _dhtVersion);
        }

        protected DhtMessage(BEncodedDictionary dictionary)
        {
            Properties = dictionary;
        }

        public BEncodedString ClientVersion
        {
            get
            {
                BEncodedValue val;
                if (Properties.TryGetValue(_versionKey, out val))
                    return (BEncodedString)val;
                return _emptyString;
            }
        }

        internal abstract NodeId Id { get; }

        public BEncodedString MessageType => (BEncodedString)Properties[_messageTypeKey];

        public BEncodedValue TransactionId
        {
            get { return Properties[_transactionIdKey]; }
            set { Properties[_transactionIdKey] = value; }
        }

        protected override int TotalPacketLength => Properties.LengthInBytes();

        protected int CheckWritten(int written)
        {
            if (written != TotalPacketLength)
                throw new Exception("Message encoded incorrectly. Incorrect number of bytes written");
            return written;
        }

        public override void Decode(Stream stream)
        {
            Properties = BEncodedValue.Decode<BEncodedDictionary>(stream, false);
        }

        public override void Encode(Stream stream)
        {
            Properties.Encode(stream);
        }

        public virtual void Handle(DhtEngine engine, Node node)
        {
            node.Seen();
        }
    }
}