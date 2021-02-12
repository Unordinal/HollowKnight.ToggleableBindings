#nullable enable

using System.Collections.Generic;

namespace ToggleableBindings.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue? GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = default)
        {
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}