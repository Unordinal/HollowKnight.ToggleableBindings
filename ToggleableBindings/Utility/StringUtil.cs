#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace ToggleableBindings.Utility
{
    public static class StringUtil
    {
        public static bool IsNullOrEmpty([NotNullWhen(false)] string? value)
        {
            return value is null || value.Length == 0;
        }

        public static bool IsNullOrWhiteSpace([NotNullWhen(false)] string? value)
        {
            return value is null || value.All(char.IsWhiteSpace);
        }

        [return: NotNullIfNotNull("value")]
        public static string? RemoveWhiteSpace(string? value)
        {
            if (value is null)
                return null;

            return Create(value.Where(c => !char.IsWhiteSpace(c)));
        }

        public static string Create(IEnumerable<char>? characters)
        {
            if (characters is null)
                return string.Empty;

            return new(characters.ToArray());
        }

        public static string Join<T>(char separator, params T[] values)
        {
            return Join(separator, values.AsEnumerable());
        }

        public static string Join<T>(string? separator, params T[] values)
        {
            return Join(separator, values.AsEnumerable());
        }

        public static string Join<T>(char separator, IEnumerable<T> values)
        {
            return JoinInternal(EnumerableUtil.AsEnumerable(separator), values);
        }

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

        private static IEnumerable<T> Solo<T>(T value)
        {
            yield return value;
        }
    }
}