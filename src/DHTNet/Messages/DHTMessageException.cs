using System;
using DHTNet.Enums;

namespace DHTNet.Messages
{
    internal class DHTMessageException : Exception
    {
        public DHTMessageException(ErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public ErrorCode ErrorCode { get; }
    }
}