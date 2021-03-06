using System;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Shared.Extensions
{
    public static class EnumerableExtensions
    {
        public static string CommaSeparate<T>(this IEnumerable<T> items)
        {
            return String.Join(", ", items.Select(x => x.ToString()));
        }
    }
}
