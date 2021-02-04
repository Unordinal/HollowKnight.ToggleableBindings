using System.Collections.Generic;

namespace UnoHKUtils
{
    public static class EnumerableUtil
    {
        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> with <paramref name="value"/> as its only element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<T> Solo<T>(T value)
        {
            yield return value;
        }
    }
}