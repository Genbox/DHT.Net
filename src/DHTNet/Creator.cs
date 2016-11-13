using DHTNet.BEncode;
using DHTNet.Messages;

namespace DHTNet
{
    internal delegate Message Creator(BEncodedDictionary dictionary);
}