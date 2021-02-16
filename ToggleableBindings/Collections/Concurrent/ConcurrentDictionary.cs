#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ToggleableBindings.Collections.Concurrent
{
    /// <summary>
    /// Represents a dictionary that ensures thread safety.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    public class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly object _lock = new();
        private readonly IDictionary<TKey, TValue> _dictionary;

        public IReadOnlyCollection<TKey> Keys { get; }

        public IReadOnlyCollection<TValue> Values { get; }

        public int Count => LockAction(() => _dictionary.Count);

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => _dictionary.Keys;

        ICollection<TValue> IDictionary<TKey, TValue>.Values => _dictionary.Values;

        public TValue this[TKey key]
        {
            get => GetKey(key);
            set => SetKey(key, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDictionary{TKey, TValue}"/> class that
        /// is empty, has the default initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        public ConcurrentDictionary()
            : this(new Dictionary<TKey, TValue>()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDictionary{TKey, TValue}"/> class that
        /// is empty, has the specified initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="ConcurrentDictionary{TKey, TValue}"/> can contain.</param>
        public ConcurrentDictionary(int capacity)
            : this(new Dictionary<TKey, TValue>(capacity)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDictionary{TKey, TValue}"/> class that
        /// is empty, has the default initial capacity, and uses the specified <see cref="IEqualityComparer{T}"/>.
        /// </summary>
        /// <param name="comparer"><inheritdoc cref="Dictionary{TKey, TValue}.Dictionary(IEqualityComparer{TKey})"/></param>
        public ConcurrentDictionary(IEqualityComparer<TKey>? comparer)
            : this(new Dictionary<TKey, TValue>(comparer)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDictionary{TKey, TValue}"/> class that
        /// is empty, has the specified initial capacity, and uses the specified <see cref="IEqualityComparer{T}"/>.
        /// </summary>
        /// <param name="capacity"><inheritdoc cref="ConcurrentDictionary{TKey, TValue}.ConcurrentDictionary(int)"/></param>
        /// <param name="comparer"><inheritdoc cref="Dictionary{TKey, TValue}.Dictionary(IEqualityComparer{TKey})"/></param>
        public ConcurrentDictionary(int capacity, IEqualityComparer<TKey>? comparer)
            : this(new Dictionary<TKey, TValue>(capacity, comparer)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDictionary{TKey, TValue}"/> class that
        /// contains elements copied from the specified <see cref="IEnumerable{T}"/>,
        /// has the specified initial capacity, and uses the specified <see cref="IEqualityComparer{T}"/>.
        /// </summary>
        /// <param name="collection">The <see cref="IEnumerable{T}"/> whose elements are copied to the new <see cref="ConcurrentDictionary{TKey, TValue}"/>.</param>
        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : this(collection.ToDictionary(p => p.Key, p => p.Value)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDictionary{TKey, TValue}"/> class that
        /// contains elements copied from the specified <see cref="IEnumerable{T}"/>,
        /// has the specified initial capacity, and uses the specified <see cref="IEqualityComparer{T}"/>.
        /// </summary>
        /// <param name="collection"><inheritdoc cref="ConcurrentDictionary{TKey, TValue}.ConcurrentDictionary(IEnumerable{KeyValuePair{TKey, TValue}})"/></param>
        /// <param name="comparer"><inheritdoc cref="ConcurrentDictionary{TKey, TValue}.ConcurrentDictionary(IEqualityComparer{TKey}?)"/></param>
        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? comparer)
            : this(new Dictionary<TKey, TValue>(collection.ToDictionary(p => p.Key, p => p.Value), comparer)) { }

        private ConcurrentDictionary(Dictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
            Keys = new ReadOnlyCollection<TKey>(_dictionary.Keys);
            Values = new ReadOnlyCollection<TValue>(_dictionary.Values);
        }

        public bool ContainsKey(TKey key)
        {
            return LockAction(() => _dictionary.ContainsKey(key));
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_lock)
                return _dictionary.TryGetValue(key, out value);
        }

        public void Add(TKey key, TValue value)
        {
            LockAction(() => _dictionary.Add(key, value));
        }

        public bool Remove(TKey key)
        {
            return LockAction(() => _dictionary.Remove(key));
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            LockAction(() => _dictionary.Add(item));
        }

        public void Clear()
        {
            LockAction(() => _dictionary.Clear());
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return LockAction(() => _dictionary.Contains(item));
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            LockAction(() => _dictionary.CopyTo(array, arrayIndex));
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return LockAction(() => _dictionary.Remove(item));
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return LockAction(() => _dictionary.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private TValue GetKey(TKey key)
        {
            return LockAction(() => _dictionary[key]);
        }

        private void SetKey(TKey key, TValue value)
        {
            LockAction(() => _dictionary[key] = value);
        }

        private void LockAction(Action action)
        {
            lock (_lock)
                action();
        }

        private T LockAction<T>(Func<T> action)
        {
            lock (_lock)
                return action();
        }
    }
}