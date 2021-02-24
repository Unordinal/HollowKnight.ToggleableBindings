#nullable enable

using System.Diagnostics.CodeAnalysis;
using ToggleableBindings.Extensions;
using UnityEngine;

namespace ToggleableBindings
{
    /// <summary>
    /// Attempts to emulate a Unity prefab, for convenience sake.
    /// </summary>
    internal sealed class FakePrefab
    {
        private static readonly GameObject _prefabContainer;

        private const string NamePrefix = "[Prefab] ";
        private readonly GameObject _prefab;
        private readonly string _prefabName;
        private readonly bool _wasActive;

        /// <summary>
        /// Gets the name of this prefab without the prefix.
        /// </summary>
        public string Name => _prefabName[NamePrefix.Length..];

        /// <summary>
        /// Returns the original game object passed to this prefab.
        /// Ensure you do not modify this object or its components in any way.
        /// </summary>
        public GameObject UnsafeGameObject => _prefab;

        static FakePrefab()
        {
            _prefabContainer = new GameObject("[[Prefabs]]");
            _prefabContainer.SetActive(false);
            Object.DontDestroyOnLoad(_prefabContainer);
        }

        internal static void Unload()
        {
            Object.DestroyImmediate(_prefabContainer, true);
        }

        /// <summary>
        /// Creates a prefab out of an existing game object.
        /// <para>
        /// Note that <paramref name="original"/> will be instantiated for this prefab's internal use.
        /// This will result in the clone's Awake() method being called if the original object was
        /// active when it was passed to this constructor.
        /// <br/>
        /// If you don't want this to happen, set <paramref name="deactivateBeforeClone"/> to <see langword="true"/>.
        /// You may also deactivate the object before passing it in, but this will result in each instance of this prefab
        /// needing to be manually activated before use.
        /// </para>
        /// </summary>
        /// <param name="original">The object to create a prefab of.</param>
        /// <param name="prefabName">The name to give the prefab. This name will be passed on to each instance.</param>
        /// <param name="deactivateBeforeClone">
        /// If <see langword="true"/>, temporarily deactivates the object before creating the prefab's internal instance
        /// and then reactivates it when finished.
        /// <br>
        /// Note that this may result in the original object's OnEnable() and OnDisable() methods being called.
        /// </br>
        /// </param>
        public FakePrefab(GameObject original, string? prefabName = null, bool deactivateBeforeClone = false)
        {
            if (!original)
                throw new System.ArgumentNullException(nameof(original));

            _prefabName = NamePrefix + (prefabName ?? original.name);
            _wasActive = original.activeSelf;

            if (_wasActive && deactivateBeforeClone)
                original.SetActive(false);

            _prefab = Object.Instantiate(original);
            _prefab.name = _prefabName;
            _prefab.SetParent(_prefabContainer);

            if (_wasActive && deactivateBeforeClone)
            {
                _prefab.SetActive(true);
                original.SetActive(true);
            }
        }

        /// <summary>
        /// Clones the prefab and returns the new game object instance.
        /// The value of <see cref="GameObject.activeSelf"/> will be the same value as the original object used to create the prefab.
        /// </summary>
        /// <param name="parent">If not <see langword="null"/>, the parent that will be assigned to the new object.</param>
        /// <param name="instantiateInWorldSpace"><inheritdoc cref="Object.Instantiate(Object, Transform, bool)" path="/param[5]"/></param>
        /// <returns>The instantiated clone.</returns>
        public GameObject Instantiate(Transform? parent = null, bool instantiateInWorldSpace = true)
        {
            var output = Object.Instantiate(_prefab, parent, instantiateInWorldSpace);
            output.name = _prefabName[NamePrefix.Length..];
            if (output.activeSelf != _wasActive)
                output.SetActive(_wasActive);

            return output;
        }

        public static implicit operator bool(FakePrefab value) => value != null;
    }
}