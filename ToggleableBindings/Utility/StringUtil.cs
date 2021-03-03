#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace ToggleableBindings.Utility
{
    /// <summary>
    /// Utility class for working with <see cref="string"/> objects.
    /// </summary>
    public static class StringUtil
    {
        /// <summary>
        /// Checks whether the given string is <see langword="null"/> or has a length of zero.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns><see langword="true"/> if <paramref name="value"/> is <see langword="null"/> or has a length of zero.</returns>
        public static bool IsNullOrEmpty([NotNullWhen(false)] string? value)
        {
            return value is null || value.Length == 0;
        }

        /// <summary>
        /// Checks whether the given string is <see langword="null"/> or consists solely of white space characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns><see langword="true"/> if <paramref name="value"/> is <see langword="null"/> or consists solely of white space characters.</returns>
        public static bool IsNullOrWhiteSpace([NotNullWhen(false)] string? value)
        {
            return value is null || value.All(char.IsWhiteSpace);
        }

        /// <summary>
        /// Returns a new string that is a copy of the given string with all white space characters removed.
        /// </summary>
        /// <param name="value">The string to use.</param>
        /// <returns>A copy of <paramref name="value"/> with all white space characters removed.</returns>
        [return: NotNullIfNotNull("value")]
        public static string? RemoveWhiteSpace(string? value)
        {
            if (value is null)
                return null;

            return Create(value.Where(c => !char.IsWhiteSpace(c)));
        }

        /// <summary>
        /// Creates a new string from an <see cref="IEnumerable{T}"/> of type <see cref="char"/>.
        /// </summary>
        /// <param name="characters">The collection of characters to use.</param>
        /// <returns>A new string that consists of the given characters, or <see cref="string.Empty"/> if <paramref name="characters"/> is <see langword="null"/>.</returns>
        public static string Create(IEnumerable<char>? characters)
        {
            if (characters is null)
                return string.Empty;

            return new(characters.ToArray());
        }

        /// <summary>
        /// Concatenates the specified separator between the string representations of the given values.
        /// </summary>
        /// <typeparam name="T">The type of values.</typeparam>
        /// <param name="separator">The separator to use.</param>
        /// <param name="values">The values to concatenate the string representations of.</param>
        /// <returns>A string that consists of the string representations of <paramref name="values"/> concatenated by <paramref name="separator"/>.</returns>
        public static string Join<T>(char separator, params T[] values)
        {
            return Join(separator, values.AsEnumerable());
        }

        /// <inheritdoc cref="Join{T}(char, T[])"/>
        public static string Join<T>(string? separator, params T[] values)
        {
            return Join(separator, values.AsEnumerable());
        }

        /// <inheritdoc cref="Join{T}(char, T[])"/>
        public static string Join<T>(char separator, IEnumerable<T> values)
        {
            return JoinInternal(EnumerableUtil.AsEnumerable(separator), values);
        }

        /// <inheritdoc cref="Join{T}(char, T[])"/>
        public static string Join<T>(string? separator, IEnumerable<T> values)
        {
            return JoinInternal(separator, values);
        }

        private static string JoinInternal<T>(IEnumerable<char>? separator, IEnumerable<T> values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));

            var fullSep = separator?.ToArray() ?? ArrayUtil.Empty<char>();
            using (var en = values.GetEnumerator())
            {
                if (!en.MoveNext())
                    return string.Empty;

                T curr = en.Current;

                string? first = curr?.ToString();

                if (!en.MoveNext())
                    return first ?? string.Empty;

                var result = new StringBuilder(256);
                result.Append(first);

                do
                {
                    curr = en.Current;
                    result.Append(fullSep);

                    if (curr is not null)
                        result.Append(curr.ToString());
                }
                while (en.MoveNext());

                return result.ToString();
            }
        }
    }
}