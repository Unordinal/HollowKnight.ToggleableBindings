using System;
using System.Collections.Generic;

namespace UnoHKUtils.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            HashSet<TResult> resultsSet = new();
            foreach (var item in source)
            {
                if (resultsSet.Add(selector(item)))
                    yield return item;
            }
        }

        /// <summary>
        /// Filters out the first item in the <see cref="IEnumerable{T}"/> that matches the specified <paramref name="predicate"/>.
        /// </summary>
        /// <typeparam name="T">The type of the items in the collection.</typeparam>
        /// <param name="source"></param>
        /// <param name="predicate">The predicate to match against.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> with the first instance that matched the <paramref name="predicate"/> removed.</returns>
        public static IEnumerable<T> WithoutFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            bool removed = false;
            foreach (var item in source)
            {
                if (!removed && (removed = predicate(item)))
                    continue;

                yield return item;
            }
        }

        /// <summary>
        /// Filters out the first item in the <see cref="IEnumerable{T}"/> that matches the specified <paramref name="predicate"/> with index.
        /// </summary>
        /// <param name="predicate">The predicate with index to match against.</param>
        ///
        /// <inheritdoc cref="WithoutFirst{T}(IEnumerable{T}, Func{T, bool})"/>
        public static IEnumerable<T> WithoutFirst<T>(this IEnumerable<T> source, Func<T, int, bool> predicate)
        {
            int index = 0;
            bool removed = false;
            foreach (var item in source)
            {
                if (!removed && (removed = predicate(item, index++)))
                    continue;

                yield return item;
            }
        }
    }
}