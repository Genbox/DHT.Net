//
// Peer.cs
//
// Authors:
//   Alan McGovern alan.mcgovern@gmail.com
//
// Copyright (C) 2006 Alan McGovern
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
using System.Collections.Generic;
using System.Net;
using System.Text;
using DHTNet.BEncode;

namespace DHTNet.MonoTorrent
{
    public class Peer
    {
        public override bool Equals(object obj)
        {
            return Equals(obj as Peer);
        }

        public bool Equals(Peer other)
        {
            if (other == null)
                return false;

            // FIXME: Don't compare the port, just compare the IP
            if (string.IsNullOrEmpty(PeerId) && string.IsNullOrEmpty(other.PeerId))
                return ConnectionUri.Host.Equals(other.ConnectionUri.Host);

            return PeerId == other.PeerId;
        }

        public override int GetHashCode()
        {
            return ConnectionUri.Host.GetHashCode();
        }

        public override string ToString()
        {
            return ConnectionUri.ToString();
        }

        internal byte[] CompactPeer()
        {
            byte[] data = new byte[6];
            CompactPeer(data, 0);
            return data;
        }

        internal void CompactPeer(byte[] data, int offset)
        {
            Buffer.BlockCopy(IPAddress.Parse(ConnectionUri.Host).GetAddressBytes(), 0, data, offset, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short) ConnectionUri.Port)), 0, data, offset + 4, 2);
        }

        internal void HashedPiece(bool succeeded)
        {
            if (succeeded && (RepeatedHashFails > 0))
                RepeatedHashFails--;

            if (!succeeded)
            {
                RepeatedHashFails++;
                TotalHashFails++;
            }
        }

        public static MonoTorrentCollection<Peer> Decode(BEncodedList peers)
        {
            MonoTorrentCollection<Peer> list = new MonoTorrentCollection<Peer>(peers.Count);
            foreach (BEncodedValue value in peers)
                try
                {
                    if (value is BEncodedDictionary)
                        list.Add(DecodeFromDict((BEncodedDictionary) value));
                    else if (value is BEncodedString)
                        foreach (Peer p in Decode((BEncodedString) value))
                            list.Add(p);
                }
                catch
                {
                    // If something is invalid and throws an exception, ignore it
                    // and continue decoding the rest of the peers
                }
            return list;
        }

        private static Peer DecodeFromDict(BEncodedDictionary dict)
        {
            string peerId;

            if (dict.ContainsKey("peer id"))
                peerId = dict["peer id"].ToString();
            else if (dict.ContainsKey("peer_id")) // HACK: Some trackers return "peer_id" instead of "peer id"
                peerId = dict["peer_id"].ToString();
            else
                peerId = string.Empty;

            Uri connectionUri = new Uri("tcp://" + dict["ip"] + ":" + dict["port"]);
            return new Peer(peerId, connectionUri, EncryptionTypes.All);
        }

        public static MonoTorrentCollection<Peer> Decode(BEncodedString peers)
        {
            // "Compact Response" peers are encoded in network byte order. 
            // IP's are the first four bytes
            // Ports are the following 2 bytes
            byte[] byteOrderedData = peers.TextBytes;
            int i = 0;
            ushort port;
            StringBuilder sb = new StringBuilder(27);
            MonoTorrentCollection<Peer> list = new MonoTorrentCollection<Peer>(byteOrderedData.Length / 6 + 1);
            while (i + 5 < byteOrderedData.Length)
            {
                sb.Remove(0, sb.Length);

                sb.Append("tcp://");
                sb.Append(byteOrderedData[i++]);
                sb.Append('.');
                sb.Append(byteOrderedData[i++]);
                sb.Append('.');
                sb.Append(byteOrderedData[i++]);
                sb.Append('.');
                sb.Append(byteOrderedData[i++]);

                port = (ushort) IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteOrderedData, i));
                i += 2;
                sb.Append(':');
                sb.Append(port);

                Uri uri = new Uri(sb.ToString());
                list.Add(new Peer("", uri, EncryptionTypes.All));
            }

            return list;
        }

        internal static BEncodedList Encode(IEnumerable<Peer> peers)
        {
            BEncodedList list = new BEncodedList();
            foreach (Peer p in peers)
                list.Add((BEncodedString) p.CompactPeer());
            return list;
        }

        public Uri ConnectionUri { get; }

        internal int CleanedUpCount { get; set; }

        public EncryptionTypes Encryption { get; set; }

        internal int TotalHashFails { get; private set; }

        internal string PeerId { get; set; }

        internal bool IsSeeder { get; set; }

        internal int FailedConnectionAttempts { get; set; }

        internal int LocalPort { get; set; }

        internal DateTime LastConnectionAttempt { get; set; }

        internal int RepeatedHashFails { get; private set; }

        public Peer(string peerId, Uri connectionUri)
            : this(peerId, connectionUri, EncryptionTypes.All)
        {
        }

        public Peer(string peerId, Uri connectionUri, EncryptionTypes encryption)
        {
            if (peerId == null)
                throw new ArgumentNullException(nameof(peerId));
            if (connectionUri == null)
                throw new ArgumentNullException(nameof(connectionUri));

            ConnectionUri = connectionUri;
            Encryption = encryption;
            PeerId = peerId;
        }
    }
}