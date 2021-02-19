#nullable enable

using UnityEngine;

namespace ToggleableBindings
{
    /// <summary>
    /// Represents a Unity prefab, for convenience sake.
    /// </summary>
    public sealed class Prefab
    {
        private const string NamePrefix = "[Prefab] ";
        private readonly GameObject _prefab;
        private readonly bool _wasActive;

        /// <summary>
        /// Creates a prefab out of an existing game object.
        /// </summary>
        /// <param name="original"></param>
        public Prefab(GameObject original, string? prefabName = null)
        {
            _wasActive = original.activeSelf;

            _prefab = Object.Instantiate(original);
            _prefab.name = NamePrefix + (prefabName ?? original.name);
            _prefab.SetActive(false);
            Object.DontDestroyOnLoad(_prefab);
        }

        /// <summary>
        /// Clones the prefab and returns the new game object instance.
        /// </summary>
        /// <param name="parent">If not <see langword="null"/>, the parent that will be assigned to the new object.</param>
        /// <param name="instantiateInWorldSpace"><inheritdoc cref="Object.Instantiate(Object, Transform, bool)" path="/param[5]"/></param>
        /// <returns>The instantiated clone.</returns>
        public GameObject Instantiate(Transform? parent = null, bool instantiateInWorldSpace = true)
        {
            var output = Object.Instantiate(_prefab, parent, instantiateInWorldSpace);
            output.name = _prefab.name[NamePrefix.Length..];
            output.SetActive(_wasActive);

            return output;
        }
    }
}