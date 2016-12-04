using System.IO;

namespace DHTNet.Messages
{
    public abstract class Message
    {
        protected abstract int TotalPacketLength { get; }

        public abstract void Decode(Stream stream);

        public abstract void Encode(Stream stream);

        public MemoryStream Encode()
        {
            MemoryStream ms = new MemoryStream(TotalPacketLength);
            Encode(ms);
            return ms;
        }

        public byte[] EncodeBytes()
        {
            MemoryStream ms = new MemoryStream(TotalPacketLength);
            Encode(ms);
            return ms.ToArray();
        }
    }
}