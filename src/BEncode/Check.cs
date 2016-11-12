using System;

namespace DHTNet.BEncode
{
    public static class Check
    {
        private static void DoCheck(object toCheck, string name)
        {
            if (toCheck == null)
                throw new ArgumentNullException(name);
        }

        public static void Value(object value)
        {
            DoCheck(value, "value");
        }
    }
}