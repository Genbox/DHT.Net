using DHTNet.BEncode;

namespace DHTNet
{
    internal static class TransactionId
    {
        private static readonly byte[] _current = new byte[2];

        public static BEncodedString NextId()
        {
            lock (_current)
            {
                BEncodedString result = new BEncodedString((byte[]) _current.Clone());
                if (_current[0]++ == 255)
                    _current[1]++;
                return result;
            }
        }
    }
}