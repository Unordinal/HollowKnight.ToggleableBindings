using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TogglableBindings.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<object> ToGeneric(this IEnumerable collection)
        {
            return collection.ToGeneric<object>();
        }

        public static IEnumerable<T> ToGeneric<T>(this IEnumerable collection)
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