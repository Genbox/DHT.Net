using DHTNet.BEncode;
using DHTNet.Messages.Queries;

namespace DHTNet
{
    delegate Message ResponseCreator(BEncodedDictionary dictionary, QueryMessage message);
}