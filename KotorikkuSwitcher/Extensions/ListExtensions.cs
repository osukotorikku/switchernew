using System.Collections.Generic;

namespace KotorikkuSwitcher.Extensions
{
    static class ListExtensions
    {
        public static void AddRange<T>(this List<T> list, params T[] items)
        {
            foreach (var i in items)
            {
                list.Add(i);
            }
        }
    }
}
