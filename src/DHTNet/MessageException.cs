using System;

namespace DHTNet
{
    internal class MessageException : Exception
    {
        public MessageException(ErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public ErrorCode ErrorCode { get; }
    }
}