// Authors:
//   Jérémie Laval <jeremie.laval@gmail.com>
//   Alan McGovern <alan.mcgovern@gmail.com>
//
// Copyright (C) 2008 Jérémie Laval, Alan McGovern
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
using System.Collections.Generic;
using System.IO;
using System.Net;
using DHTNet.BEncode;
using DHTNet.Enums;
using DHTNet.Utils.Endian;

namespace DHTNet.Nodes
{
    /// <summary>
    /// A node is a client/server listening on a UDP port implementing the distributed hash table protocol.
    /// </summary>
    public class Node : IComparable<Node>, IEquatable<Node>
    {
        public Node(NodeId id, IPEndPoint endpoint)
        {
            EndPoint = endpoint;
            Id = id;
        }

        public IPEndPoint EndPoint { get; }

        public int FailedCount { get; internal set; }

        public NodeId Id { get; }

        public DateTime LastSeen { get; internal set; }

        // FIXME: State should be set properly as per specification.
        // i.e. it needs to take into account the 'LastSeen' property.
        // and must take into account when a node does not send us messages etc
        public NodeState State
        {
            get
            {
                if (FailedCount >= Config.MaxFailures)
                    return NodeState.Bad;

                if (LastSeen == DateTime.MinValue)
                    return NodeState.Unknown;

                return (DateTime.UtcNow - LastSeen).TotalMinutes < 15 ? NodeState.Good : NodeState.Questionable;
            }
        }

        public BEncodedString Token { get; set; }

        //To order by last seen in bucket
        public int CompareTo(Node other)
        {
            if (other == null)
                return 1;

            return LastSeen.CompareTo(other.LastSeen);
        }

        public bool Equals(Node other)
        {
            if (other == null)
                return false;

            return Id.Equals(other.Id);
        }

        internal void Seen()
        {
            FailedCount = 0;
            LastSeen = DateTime.UtcNow;
        }

        internal BEncodedString CompactAddressPort()
        {
            byte[] buffer = new byte[6];
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                CompactAddressPort(ms);
            }
            return buffer;
        }

        internal void CompactAddressPort(Stream stream)
        {
            using (EndianBinaryWriter writer = new EndianBinaryWriter(EndianBitConverter.Big, stream, true))
            {
                writer.Write(EndPoint.Address.GetAddressBytes());
                writer.Write((ushort)EndPoint.Port);
            }
        }

        internal static BEncodedString CompactAddressPort(IList<Node> peers)
        {
            byte[] buffer = new byte[peers.Count * 6];
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                foreach (Node node in peers)
                    node.CompactAddressPort(ms);
            }

            return buffer;
        }

        internal BEncodedString CompactNode()
        {
            byte[] buffer = new byte[26];
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                CompactNode(ms);
            }
            return buffer;
        }

        private void CompactNode(Stream stream)
        {
            stream.Write(Id.Bytes, 0, Id.Bytes.Length);
            CompactAddressPort(stream);
        }

        internal static BEncodedString CompactNode(IList<Node> nodes)
        {
            byte[] buffer = new byte[nodes.Count * 26];
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                foreach (Node node in nodes)
                    node.CompactNode(ms);
            }

            return buffer;
        }

        internal static Node FromCompactNode(byte[] buffer, int offset)
        {
            byte[] id = new byte[Config.HashLength];
            Buffer.BlockCopy(buffer, offset, id, 0, id.Length);
            IPAddress address = new IPAddress((uint)BitConverter.ToInt32(buffer, offset + 20));
            int port = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(buffer, offset + 24));
            return new Node(new NodeId(id), new IPEndPoint(address, port));
        }

        internal static IEnumerable<Node> FromCompactNode(byte[] buffer)
        {
            for (int i = 0; i + 26 <= buffer.Length; i += 26)
                yield return FromCompactNode(buffer, i);
        }

        internal static IEnumerable<Node> FromCompactNode(BEncodedString nodes)
        {
            return FromCompactNode(nodes.TextBytes);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Node);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return Id.ToString();
        }

        internal static IEnumerable<Node> CloserNodes(NodeId target, SortedList<NodeId, NodeId> currentNodes, IEnumerable<Node> newNodes, int maxNodes)
        {
            foreach (Node node in newNodes)
            {
                if (currentNodes.ContainsValue(node.Id))
                    continue;

                NodeId distance = node.Id.Xor(target);
                if (currentNodes.Count < maxNodes)
                {
                    currentNodes.Add(distance, node.Id);
                }
                else if (distance < currentNodes.Keys[currentNodes.Count - 1])
                {
                    currentNodes.RemoveAt(currentNodes.Count - 1);
                    currentNodes.Add(distance, node.Id);
                }
                else
                {
                    continue;
                }
                yield return node;
            }
        }
    }
}