using System;
using System.Collections.Generic;

namespace ToggleableBindings.Utility
{
    /// <summary>
    /// Utility for working with arrays.
    /// </summary>
    public static class ArrayUtil
    {
        private static readonly Dictionary<Type, Array> _emptyArrayCache = new();

        /// <summary>
        /// Gets an empty array of type <typeparamref name="T"/> and caches it for further calls.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <returns>An empty array of type <typeparamref name="T"/>.</returns>
        public static T[] Empty<T>()
        {
            Type typeOfT = typeof(T);
            Array output;
            if (_emptyArrayCache.TryGetValue(typeOfT, out var empty))
                output = empty;
            else
                output = _emptyArrayCache[typeOfT] = new T[0];

            return (T[])output;
        }
    }
}