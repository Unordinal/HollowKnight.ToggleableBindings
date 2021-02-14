using System.Collections.Generic;

namespace ToggleableBindings.Collections
{
    /// <summary>
    /// Provides a read-only view of a dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    public interface IReadOnlyDictionary<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    {
        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <inheritdoc cref="IDictionary{TKey, TValue}.this[TKey]"/>
        TValue this[TKey key] { get; }

        /// <summary>
        /// Gets a read-only view of the keys in the dictionary.
        /// </summary>
        IReadOnlyCollection<TKey> Keys { get; }

        /// <summary>
        /// Gets a read-only view of the values in the dictionary.
        /// </summary>
        IReadOnlyCollection<TValue> Values { get; }

        /// <inheritdoc cref="IDictionary{TKey, TValue}.ContainsKey(TKey)"/>
        bool ContainsKey(TKey key);

        /// <inheritdoc cref="IDictionary{TKey, TValue}.ContainsKey(TKey)"/>
        bool TryGetValue(TKey key, out TValue value);
    }
}