#nullable enable

using System;
using UnityEngine;

namespace ToggleableBindings.Extensions
{
    public static class GameObjectExtensions
    {
        public static GameObject? GetParent(this GameObject gameObject)
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            Transform? parentTransform = gameObject.transform.parent;
            if (parentTransform)
                return parentTransform.gameObject;

            return null;
        }

        public static void SetParent(this GameObject gameObject, GameObject parent, bool worldPositionStays = true)
        {
            if (!gameObject)
                throw new ArgumentNullException(nameof(gameObject));

            if (!parent)
                throw new ArgumentNullException(nameof(parent));

            gameObject.transform.SetParent(parent.transform, worldPositionStays);
        }

        public static string ListHierarchy(this GameObject gameObject)
        {
            string hierarchy = gameObject.name;
            var parent = gameObject.GetParent();
            if (parent != null)
                hierarchy += "->" + parent.ListHierarchy();
            return hierarchy;
        }

        public static T GetComponentInChild<T>(this GameObject gameObject, string childPath) where T : Component
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            if (childPath is null)
                throw new ArgumentNullException(nameof(childPath));

            return gameObject.FindChild(childPath).GetComponent<T>();
        }

        public static GameObject FindChild(this GameObject gameObject, string childPath)
        {
            if (!gameObject)
                throw new ArgumentNullException(nameof(gameObject));

            return gameObject.transform.Find(childPath)?.gameObject ?? throw new ArgumentException($"Couldn't find GameObject via path '{childPath}'.");
        }

        /// <summary>
        /// Removes a component of the specified type from this game object.
        /// </summary>
        /// <param name="go">The game object to remove a component from.</param>
        /// <param name="componentType">The type of component.</param>
        public static void RemoveComponent(this GameObject go, Type componentType)
        {
            if (!go)
                throw new ArgumentNullException(nameof(go));

            if (!componentType.IsAssignableTo(typeof(Component)))
                throw new ArgumentException("The type was not a valid component type.", nameof(componentType));

            var component = go.GetComponent(componentType);
            if (component)
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
            if (!go)
                throw new ArgumentNullException(nameof(go));

            return go.gameObject.scene.rootCount == 0 && !go.activeInHierarchy;
        }
    }
}