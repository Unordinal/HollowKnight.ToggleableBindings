#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnoHKUtils.Extensions
{
    public static class StringExtensions
    {
        public static string Join(string? separator, IEnumerable<object?> values)
        {
            return Join(separator, values.Select(o => o?.ToString()));
        }

        public static string Join(string? separator, IEnumerable<string?> values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));

            using (IEnumerator<string?> en = values.GetEnumerator())
            {
                if (!en.MoveNext())
                    return string.Empty;

                string? first = en.Current;
                if (!en.MoveNext())
                    return first ?? string.Empty;

                var result = new StringBuilder();
                result.Append(first);

                do
                {
                    result.Append(separator);
                    result.Append(en.Current);
                }
                while (en.MoveNext());

                return result.ToString();
            }
        }
    }
}