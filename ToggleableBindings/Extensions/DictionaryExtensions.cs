#nullable enable

using System.Collections.Generic;

namespace ToggleableBindings.Extensions
{
    /// <summary>
    /// Extensions for working with <see cref="IDictionary{TKey, TValue}"/> objects.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets the value at the specified key or <paramref name="defaultValue"/> if the key was not found.
        /// </summary>
        /// <typeparam name="TKey">The type of keys.</typeparam>
        /// <typeparam name="TValue">The type of values.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key to get.</param>
        /// <param name="defaultValue">The default value to return if <paramref name="key"/> did not exist within the dictionary.</param>
        /// <returns>The value at the specified key, or <paramref name="defaultValue"/> if <paramref name="key"/> did not exist in <paramref name="dictionary"/>.</returns>
        public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = default)
        {
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}