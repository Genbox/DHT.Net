// Authors:
//   J�r�mie Laval <jeremie.laval@gmail.com>
//   Alan McGovern <alan.mcgovern@gmail.com>
//
// Copyright (C) 2008 J�r�mie Laval, Alan McGovern
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
using DHTNet.Utils;

namespace DHTNet.Nodes
{
    /// <summary>
    /// Node IDs are chosen at random from the same 160-bit space as BitTorrent infohashes. Usually based on the SHA1 cryptographic hash algorithm.
    /// </summary>
    public class NodeId : IEquatable<NodeId>, IComparable<NodeId>, IComparable
    {
        private readonly BigInteger _value;

        public static readonly NodeId Minimum = new NodeId(new byte[20]);
        public static readonly NodeId Maximum = new NodeId(new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 });

        public NodeId()
        {
            //DHT.NET: We don't set _value or Bytes in this constructor to weed out bugs.
        }

        public NodeId(byte[] value)
            : this(new BigInteger(value))
        {
            if (value.Length != 20)
                throw new ArgumentException("Id of length " + value.Length + " is invalid.");

            Bytes = value;
        }

        public NodeId(InfoHash infoHash)
            : this(infoHash.ToArray())
        {
        }

        private NodeId(BigInteger value)
        {
            _value = value;

            //DHT.NET: Not entirely sure this is needed, but it is used in the MessageWriter, which means we possibly send out corrupt messages
            if (Bytes == null)
                Bytes = value.GetBytes(Config.HashLength);
        }

        public byte[] Bytes { get; }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as NodeId);
        }

        public int CompareTo(NodeId other)
        {
            if ((object)other == null)
                return 1;

            BigInteger.Sign s = _value.Compare(other._value);
            if (s == BigInteger.Sign.Zero)
                return 0;
            if (s == BigInteger.Sign.Positive)
                return 1;
            return -1;
        }

        public bool Equals(NodeId other)
        {
            if ((object)other == null)
                return false;

            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as NodeId);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString(16);
        }

        /// <summary>
        /// In Kademlia, the distance metric is XOR and the result is interpreted as an unsigned integer. distance(A,B) = |A xor B| Smaller values are closer.
        /// </summary>
        internal NodeId Xor(NodeId other)
        {
            return new NodeId(_value.Xor(other._value));
        }

        public static implicit operator NodeId(int value)
        {
            return new NodeId(new BigInteger((uint)value));
        }

        public static NodeId operator -(NodeId first)
        {
            CheckArguments(first);
            return new NodeId(first._value);
        }

        public static NodeId operator -(NodeId first, NodeId second)
        {
            CheckArguments(first, second);
            return new NodeId(first._value - second._value);
        }

        public static bool operator >(NodeId first, NodeId second)
        {
            CheckArguments(first, second);
            return first._value > second._value;
        }

        public static bool operator <(NodeId first, NodeId second)
        {
            CheckArguments(first, second);
            return first._value < second._value;
        }

        public static bool operator <=(NodeId first, NodeId second)
        {
            CheckArguments(first, second);
            return (first < second) || (first == second);
        }

        public static bool operator >=(NodeId first, NodeId second)
        {
            CheckArguments(first, second);
            return (first > second) || (first == second);
        }

        public static NodeId operator +(NodeId first, NodeId second)
        {
            CheckArguments(first, second);
            return new NodeId(first._value + second._value);
        }

        public static NodeId operator /(NodeId first, NodeId second)
        {
            CheckArguments(first, second);
            return new NodeId(first._value / second._value);
        }

        private static void CheckArguments(NodeId first)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
        }

        private static void CheckArguments(NodeId first, NodeId second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));
        }

        public static bool operator ==(NodeId first, NodeId second)
        {
            if ((object)first == null)
                return (object)second == null;
            if ((object)second == null)
                return false;

            return first._value == second._value;
        }

        public static bool operator !=(NodeId first, NodeId second)
        {
            return !(first == second);
        }

        internal BEncodedString BencodedString()
        {
            return new BEncodedString(_value.GetBytes(Config.HashLength));
        }

        public static NodeId Create()
        {
            return new NodeId(Toolbox.GetRandomBytes(Config.HashLength));
        }
    }
}