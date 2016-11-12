using System;
using System.Diagnostics;
using System.Text;

namespace DHTNet.MonoTorrent
{
    public static class Logger
    {
        private static readonly object LockObj = new object();
        private static readonly StringBuilder sb = new StringBuilder();

        [Conditional("DO_NOT_ENABLE")]
        internal static void Log(IConnection connection, string message)
        {
            Log(connection, message, null);
        }

        [Conditional("DO_NOT_ENABLE")]
        internal static void Log(IConnection connection, string message, params object[] formatting)
        {
            lock (LockObj)
            {
                sb.Remove(0, sb.Length);
                sb.Append(Environment.TickCount);
                sb.Append(": ");

                if (connection != null)
                    sb.Append(connection.EndPoint);

                if (formatting != null)
                    sb.Append(string.Format(message, formatting));
                else
                    sb.Append(message);
                string s = sb.ToString();
            }
        }
    }
}