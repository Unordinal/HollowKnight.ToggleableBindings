#nullable enable

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace ToggleableBindings.Extensions
{
    public static class HashtableExtensions
    {
        public static TValue? GetValue<TKey, TValue>(this Hashtable hashtable, TKey? key)
        {
            return hashtable[key] is TValue value ? value : default;
        }

        public static bool TryGetValue<TKey, TValue>(this Hashtable hashtable, TKey? key, [NotNullWhen(true)] out TValue? value)
        {
            value = default;

            if (hashtable[key] is TValue result)
            {
                value = result;
                return true;
            }

            return false;
        }
    }
}