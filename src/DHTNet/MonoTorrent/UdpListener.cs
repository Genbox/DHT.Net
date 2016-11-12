//
// UdpListener.cs
//
// Authors:
//   Alan McGovern <alan.mcgovern@gmail.com>
//
// Copyright (C) 2008 Alan McGovern
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Net;
using System.Net.Sockets;

namespace DHTNet.MonoTorrent
{
    public abstract class UdpListener : Listener
    {
        private UdpClient client;

        protected UdpListener(IPEndPoint endpoint)
            : base(endpoint)
        {

        }

        protected abstract void OnMessageReceived(byte[] buffer, IPEndPoint endpoint);

        public virtual void Send(byte[] buffer, IPEndPoint endpoint)
        {
            try
            {
                if (endpoint.Address != IPAddress.Any)
                    client.SendAsync(buffer, buffer.Length, endpoint).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.Log(null, "UdpListener could not send message: {0}", ex);
            }
        }

        public override void Start()
        {
            try
            {
                client = new UdpClient(Endpoint);
                RaiseStatusChanged(ListenerStatus.Listening);

                while (true)
                {
                    try
                    {
                        StartReceive();
                    }
                    catch (ObjectDisposedException)
                    {
                        // Ignore, we're finished!
                    }
                    catch (SocketException ex)
                    {
                        // If the destination computer closes the connection
                        // we get error code 10054 (ConnectionReset). We need to keep receiving on
                        // the socket until we clear all the error states
                        if (ex.SocketErrorCode == SocketError.ConnectionReset)
                        {
                            while (true)
                            {
                                try
                                {
                                    StartReceive();
                                    return;
                                }
                                catch (ObjectDisposedException)
                                {
                                    return;
                                }
                                catch (SocketException e)
                                {
                                    if (e.SocketErrorCode != SocketError.ConnectionReset)
                                        return;
                                }
                            }
                        }
                    }
                }
            }
            catch (SocketException)
            {
                RaiseStatusChanged(ListenerStatus.PortNotFree);
            }
            catch (ObjectDisposedException)
            {
                // Do Nothing
            }
        }

        private void StartReceive()
        {
            UdpReceiveResult result = client.ReceiveAsync().GetAwaiter().GetResult();
            IPEndPoint e = new IPEndPoint(IPAddress.Any, Endpoint.Port);
            OnMessageReceived(result.Buffer, e);
        }

        public override void Stop()
        {
            try
            {
                client.Dispose();
            }
            catch
            {
                // FIXME: Not needed
            }
        }
    }
}
