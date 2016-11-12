using System;

namespace Mono.Ssdp.Internal
{
    internal delegate bool TimeoutHandler (object state, ref TimeSpan interval);
}