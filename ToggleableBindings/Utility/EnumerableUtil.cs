using System.Collections.Generic;
using System.Linq;

namespace ToggleableBindings.Utility
{
    /// <summary>
    /// Utility class for working with <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class EnumerableUtil
    {
        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> that contains a single value.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="value">The value the <see cref="IEnumerable{T}"/> will contain.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> with <paramref name="value"/> as its only element.</returns>
        public static IEnumerable<T> AsEnumerable<T>(T value)
        {
            yield return value;
        }

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> that contains the specified values.
        /// </summary>
        /// <typeparam name="T">The type of values.</typeparam>
        /// <param name="values">The values the <see cref="IEnumerable{T}"/> will contain.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that contains the specified values.</returns>
        public static IEnumerable<T> AsEnumerable<T>(params T[] values)
        {
            return values ?? Enumerable.Empty<T>();
        }
    }
}