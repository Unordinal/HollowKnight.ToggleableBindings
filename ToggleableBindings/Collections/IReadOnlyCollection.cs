using System.Collections.Generic;

namespace ToggleableBindings.Collections
{
    /// <summary>
    /// Provides a read-only view of a generic collection.
    /// </summary>
    /// <typeparam name="T">The type of the items within the collection.</typeparam>
    public interface IReadOnlyCollection<T> : IEnumerable<T>
    {
        /// <inheritdoc cref="ICollection{T}.Count"/>
        int Count { get; }

        /// <inheritdoc cref="ICollection{T}.Contains(T)"/>
        bool Contains(T item);
    }
}