using MonoTorrent.BEncoding;

namespace MonoTorrent.Dht.Messages
{
    delegate Message ResponseCreator(BEncodedDictionary dictionary, QueryMessage message);
}