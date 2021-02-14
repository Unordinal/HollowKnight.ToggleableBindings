using System;
using System.Collections;
using System.Collections.Generic;

namespace ToggleableBindings.Collections
{
    /// <inheritdoc cref="IReadOnlyCollection{T}"/>
    public class ReadOnlyCollection<T> : IReadOnlyCollection<T>
    {
        private readonly ICollection<T> _collection;

        /// <inheritdoc cref="ICollection{T}.Count"/>
        public int Count => _collection.Count;

        /// <summary>
        /// Creates a new read-only collection by wrapping the specified collection.
        /// </summary>
        /// <param name="collection">The collection to wrap.</param>
        public ReadOnlyCollection(ICollection<T> collection)
        {
            if (collection is null)
                throw new ArgumentNullException(nameof(collection));

            _collection = collection;
        }

        public bool Contains(T item)
        {
            return _collection.Contains(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}