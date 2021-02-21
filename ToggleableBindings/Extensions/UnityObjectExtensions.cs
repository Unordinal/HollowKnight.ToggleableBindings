#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace ToggleableBindings.Extensions
{
    public static class UnityObjectExtensions
    {
        [SuppressMessage("Style", "IDE0029:Use coalesce expression", Justification = "Cannot properly use coalesce with a Unity object.")]
        public static T? URef<T>(this T? unityObject) where T : UnityEngine.Object
        {
            return unityObject != null ? unityObject : null;
        }
    }
}