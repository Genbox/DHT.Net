using System.Collections.Generic;

namespace DHTNet.Utils.Extensions
{
    public static class ListExtensions
    {
        public static T TakeFirst<T>(this List<T> list)
        {
            T result = list[0];
            list.RemoveAt(0);
            return result;
        }
    }
}
