namespace DHTNet
{
    internal enum ErrorCode
    {
        GenericError = 201,
        ServerError = 202,
        ProtocolError = 203, // malformed packet, invalid arguments, or bad token
        MethodUnknown = 204 //Method Unknown
    }
}