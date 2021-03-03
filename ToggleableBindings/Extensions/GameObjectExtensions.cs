#nullable enable

using System;
using UnityEngine;

namespace ToggleableBindings.Extensions
{
    /// <summary>
    /// Extensions for Unity game objects.
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Gets the parent of this game object.
        /// </summary>
        /// <param name="gameObject">The game object.</param>
        /// <returns>The parent of the object, or <see langword="null"/> if the object has no parent.</returns>
        public static GameObject? GetParent(this GameObject gameObject)
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            Transform? parentTransform = gameObject.transform.parent;
            if (parentTransform)
                return parentTransform.gameObject;

            return null;
        }

        /// <summary>
        /// Sets the parent of this game object.
        /// </summary>
        /// <param name="gameObject">The game object.</param>
        /// <param name="parent">The new parent of the object.</param>
        /// <param name="worldPositionStays">Whether the object should keep its world position.</param>
        public static void SetParent(this GameObject gameObject, GameObject parent, bool worldPositionStays = true)
        {
            if (!gameObject)
                throw new ArgumentNullException(nameof(gameObject));

            if (!parent)
                throw new ArgumentNullException(nameof(parent));

            gameObject.transform.SetParent(parent.transform, worldPositionStays);
        }

        /// <summary>
        /// Gets the component of the given type on the child at the specified path in this game object.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <param name="gameObject">The game object.</param>
        /// <param name="childPath">The path of the child in this game object.</param>
        /// <returns>The component of the given type on the found child.</returns>
        public static T GetComponentInChild<T>(this GameObject gameObject, string childPath) where T : Component
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            if (childPath is null)
                throw new ArgumentNullException(nameof(childPath));

            return gameObject.FindChild(childPath).GetComponent<T>();
        }

        /// <summary>
        /// Finds the child of this game object. Shorthand for <see cref="Transform.Find(string)"/>.
        /// </summary>
        /// <param name="gameObject">The game object.</param>
        /// <param name="childPath">The path of the child to find.</param>
        /// <returns>The game object at the specified path.</returns>
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
        /// Checks to see if this game object is a prefab. Not tested.
        /// </summary>
        /// <param name="go">The object to check.</param>
        /// <returns><see langword="true"/> if this object is a Unity prefab; otherwise, <see langword="false"/>.</returns>
        public static bool IsPrefab(this GameObject go)
        {
            if (!go)
                throw new ArgumentNullException(nameof(go));

            return go.gameObject.scene.rootCount == 0 && !go.activeInHierarchy;
        }

        internal static string ListHierarchy(this GameObject gameObject)
        {
            string hierarchy = gameObject.name;
            var parent = gameObject.GetParent();
            if (parent != null)
                hierarchy += "->" + parent.ListHierarchy();
            return hierarchy;
        }
    }
}