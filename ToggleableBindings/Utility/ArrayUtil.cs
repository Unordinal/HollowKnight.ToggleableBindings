using System;
using System.Collections.Generic;

namespace ToggleableBindings.Utility
{
    public static class ArrayUtil
    {
        private static readonly Dictionary<Type, Array> _emptyArrayCache = new();

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