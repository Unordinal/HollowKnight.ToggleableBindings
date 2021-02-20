#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
using UnityEngine;
using Logger = Modding.Logger;

namespace ToggleableBindings.Extensions
{
    public static class GameObjectExtensions
    {
        public static GameObject? GetParent(this GameObject gameObject)
        {
            return gameObject?.transform?.parent?.gameObject;
        }

        public static void SetParent(this GameObject gameObject, GameObject parent, bool worldPositionStays = true)
        {
            if (gameObject is null)
                throw new ArgumentNullException(nameof(gameObject));

            if (parent is null)
                throw new ArgumentNullException(nameof(parent));

            gameObject.transform.SetParent(parent.transform, worldPositionStays);
        }

        public static IEnumerable<GameObject> GetChildren(this GameObject gameObject)
        {
            foreach (var child in gameObject.transform.GetComponentsInChildren<Transform>())
                yield return child.gameObject;
        }

        public static string ListHierarchy(this GameObject gameObject)
        {
            string hierarchy = gameObject.name;
            var parent = gameObject.GetParent();
            if (parent is not null)
                hierarchy += "->" + parent.ListHierarchy();
            return hierarchy;
        }

        public static GameObject FindChild(this GameObject gameObject, string childPath)
        {
            if (gameObject is null)
                throw new ArgumentNullException(nameof(gameObject));

            return gameObject.transform.Find(childPath).gameObject ?? throw new Exception("Couldn't find GameObject via path.");
        }

        /// <summary>
        /// Removes a component of the specified type from this game object.
        /// </summary>
        /// <param name="go">The game object to remove a component from.</param>
        /// <param name="componentType">The type of component.</param>
        public static void RemoveComponent(this GameObject go, Type componentType)
        {
            if (go is null)
                throw new ArgumentNullException(nameof(go));

            if (!componentType.IsAssignableTo(typeof(Component)))
                throw new ArgumentException("The type was not a valid component type.", nameof(componentType));

            var component = go.GetComponent(componentType);
            if (component is not null)
                UnityEngine.Object.DestroyImmediate(component, true);
        }

        /// <typeparam name="T">The type of component to remove.</typeparam>
        /// <inheritdoc cref="RemoveComponent(GameObject, Type)"/>
        public static void RemoveComponent<T>(this GameObject go) where T : Component
        {
            RemoveComponent(go, typeof(T));
        }

        /// <summary>
        /// Checks to see if this game object is a prefab.
        /// </summary>
        /// <param name="go">The object to check.</param>
        /// <returns><see langword="true"/> if this object is a Unity prefab; otherwise, <see langword="false"/>.</returns>
        public static bool IsPrefab(this GameObject go)
        {
            if (go is null)
                throw new ArgumentNullException(nameof(go));

            return go.gameObject.scene.rootCount == 0 && !go.activeInHierarchy;
        }
    }
}
