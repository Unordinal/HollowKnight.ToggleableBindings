#nullable enable

using System;

namespace UnoHKUtils.Extensions
{
    internal static class ThrowHelper
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if <paramref name="obj"/> is <see langword="null"/>,
        /// using <paramref name="paramName"/> in the exception's constructor.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="obj"/>.</typeparam>
        /// <param name="obj">The object to check for null.</param>
        /// <param name="paramName">The name of the parameter.</param>
        public static void ThrowIfNull<T>(this T obj, string paramName)
        {
            if (obj is null)
                throw new ArgumentNullException(paramName);
        }
    }
}