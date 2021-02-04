#nullable enable

using UnityEngine;

namespace UnoHKUtils.Extensions.Unity
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Gets the parent object of this <see cref="GameObject"/>.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static GameObject? GetParent(this GameObject gameObject)
        {
            return gameObject?.transform?.parent?.gameObject;
        }

        /// <summary>
        /// Gets the root object of this <see cref="GameObject"/>.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static GameObject? GetRoot(this GameObject gameObject)
        {
            return gameObject?.transform?.root?.gameObject;
        }
    }
}