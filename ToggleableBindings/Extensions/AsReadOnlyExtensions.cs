using System;
using System.Collections.Generic;
using ToggleableBindings.Collections;

namespace ToggleableBindings.Extensions
{
    /// <summary>
    /// Extensions to wrap various types of collections with read-only collections.
    /// </summary>
    public static class AsReadOnlyExtensions
    {
        /// <summary>
        /// Returns a read-only collection wrapper for the current collection.
        /// </summary>
        /// <typeparam name="T">The type of values in the collection.</typeparam>
        /// <param name="collection">The collection to wrap.</param>
        /// <returns>An object that acts as a read-only wrapper around the current <see cref="ICollection{T}"/>.</returns>
        public static IReadOnlyCollection<T> AsReadOnly<T>(this ICollection<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            return new ReadOnlyCollection<T>(collection);
        }

        /// <summary>
        /// Returns a read-only list wrapper for the current list.
        /// </summary>
        /// <typeparam name="T">The type of values in the list.</typeparam>
        /// <param name="list">The list to wrap.</param>
        /// <returns>An object that acts as a read-only wrapper around the current <see cref="IList{T}"/>.</returns>
        public static IReadOnlyList<T> AsReadOnly<T>(this IList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            return new ReadOnlyList<T>(list);
        }

        /// <summary>
        /// Returns a read-only dictionary wrapper for the current dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to wrap.</param>
        /// <returns>An object that acts as a read-only wrapper around the current <see cref="IDictionary{TKey, TValue}"/>.</returns>
        public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            return new ReadOnlyDictionary<TKey, TValue>(dictionary);
        }
    }
}