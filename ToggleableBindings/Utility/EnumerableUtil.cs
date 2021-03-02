using System.Collections.Generic;
using System.Linq;

namespace ToggleableBindings.Utility
{
    public static class EnumerableUtil
    {
        public static IEnumerable<T> AsEnumerable<T>(T value)
        {
            yield return value;
        }

        public static IEnumerable<T> AsEnumerable<T>(params T[] values)
        {
            return values ?? Enumerable.Empty<T>();
        }
    }
}