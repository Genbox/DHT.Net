using System;

namespace BEncode
{
    public static class Check
    {
        static void DoCheck(object toCheck, string name)
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