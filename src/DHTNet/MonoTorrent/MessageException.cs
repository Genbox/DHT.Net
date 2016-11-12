using System;

namespace DHTNet.MonoTorrent
{
    public class MessageException : TorrentException
    {
        public MessageException()
        {
        }


        public MessageException(string message)
            : base(message)
        {
        }


        public MessageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}