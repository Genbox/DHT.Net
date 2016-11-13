using DHTNet.BEncode;
using DHTNet.Messages;
using DHTNet.Messages.Queries;

namespace DHTNet
{
    internal delegate Message ResponseCreator(BEncodedDictionary dictionary, QueryMessage message);
}