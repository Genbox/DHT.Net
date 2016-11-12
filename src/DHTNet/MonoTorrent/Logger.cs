using System;
using System.Text;

namespace DHTNet.MonoTorrent
{
    public static class Logger
    {
        private static readonly object _lockObj = new object();
        private static readonly StringBuilder _sb = new StringBuilder();

        internal static void Log(IConnection connection, string message)
        {
            Log(connection, message, null);
        }

        internal static void Log(IConnection connection, string message, params object[] formatting)
        {
            lock (_lockObj)
            {
                _sb.Remove(0, _sb.Length);
                _sb.Append(Environment.TickCount);
                _sb.Append(": ");

                if (connection != null)
                    _sb.Append(connection.EndPoint);

                if (formatting != null)
                    _sb.Append(string.Format(message, formatting));
                else
                    _sb.Append(message);
                string s = _sb.ToString();
            }
        }
    }
}