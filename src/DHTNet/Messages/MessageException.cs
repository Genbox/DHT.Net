using System;
using DHTNet.Enums;

namespace DHTNet.Messages
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