using System;

namespace DHTNet.MonoTorrent
{
    internal delegate bool TimeoutHandler (object state, ref TimeSpan interval);
}