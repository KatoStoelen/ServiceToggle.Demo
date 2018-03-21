using System;
using System.Collections.Generic;

namespace ServiceToggle.Windsor.Extensions
{
    internal static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> @this, Action<T> action)
        {
            if (@this == null) return;

            foreach (var item in @this)
            {
                action(item);
            }
        }
    }
}
