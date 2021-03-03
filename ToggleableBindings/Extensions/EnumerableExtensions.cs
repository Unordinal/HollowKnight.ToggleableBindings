using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ToggleableBindings.Extensions
{
    /// <summary>
    /// Extensions for working with <see cref="IEnumerable"/> and <see cref="IEnumerable{T}"/> objects.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Gets the specified <see cref="IEnumerable"/> as a generic <see cref="IEnumerable{T}"/> of type <see cref="object"/>.
        /// </summary>
        /// <param name="collection">The non-generic collection.</param>
        /// <returns>A generic version of the specified collection.</returns>
        public static IEnumerable<object> AsGeneric(this IEnumerable collection)
        {
            return collection.AsGeneric<object>();
        }

        /// <summary>
        /// Gets the specified <see cref="IEnumerable"/> as a generic <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of values.</typeparam>
        /// <param name="collection">The non-generic collection</param>
        /// <returns>A generic version of the specified collection.</returns>
        public static IEnumerable<T> AsGeneric<T>(this IEnumerable collection)
        {
            foreach (T obj in collection)
                yield return obj;
        }

        /// <summary>
        /// Concatenates the specified item to the end of the given collection.
        /// </summary>
        /// <typeparam name="T">The type of values.</typeparam>
        /// <param name="collection">The collection to concatenate to.</param>
        /// <param name="item">The item to concatenate.</param>
        /// <returns>The specified enumerable with <paramref name="item"/> appended to the end.</returns>
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> collection, T item)
        {
            foreach (var elem in collection)
                yield return elem;

            yield return item;
        }
    }
}