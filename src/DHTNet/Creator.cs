using MonoTorrent.BEncoding;

namespace MonoTorrent.Dht.Messages
{
    delegate Message Creator(BEncodedDictionary dictionary);
}