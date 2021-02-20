using System;
using System.Collections;
using System.Collections.Generic;

namespace ToggleableBindings.Collections
{
    /// <inheritdoc cref="IReadOnlyList{T}"/>
    public class ReadOnlyList<T> : IReadOnlyList<T>
    {
        private readonly IList<T> _list;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="IList{T}"/>.
        /// </summary>
        public int Count => _list.Count;

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <inheritdoc cref="IList{T}.this[int]"/>
        public T this[int index] => _list[index];

        /// <summary>
        /// Creates a new read-only list by wrapping the specified <see cref="IList{T}"/>.
        /// </summary>
        /// <param name="list">The list to wrap.</param>
        public ReadOnlyList(IList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            _list = list;
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}