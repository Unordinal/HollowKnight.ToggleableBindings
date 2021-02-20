using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ToggleableBindings.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<object> AsGeneric(this IEnumerable collection)
        {
            return collection.AsGeneric<object>();
        }

        public static IEnumerable<T> AsGeneric<T>(this IEnumerable collection)
        {
            foreach (T obj in collection)
                yield return obj;
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> collection, T item)
        {
            foreach (var elem in collection)
                yield return elem;

            yield return item;
        }
    }
}