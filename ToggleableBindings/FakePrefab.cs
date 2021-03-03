#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using ToggleableBindings.Debugging;
using ToggleableBindings.Utility;
using UnityEngine;
using Object = UnityEngine.Object;

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
        /// </para>
        /// </summary>
        /// <param name="original">The object to create a prefab of.</param>
        /// <param name="prefabName">The name to give the prefab. This name will be passed on to each instance.</param>
        /// <param name="setActive">Whether to set the prefab object as active once it's initialized.</param>
        public FakePrefab(GameObject original, string? prefabName = null, bool setActive = false)
        {
            if (original == null)
                throw new ArgumentNullException(nameof(original));

            _prefabName = NamePrefix + (prefabName ?? original.name);
            _prefab = Object.Instantiate(original, _prefabContainer.transform);
            _prefab.name = _prefabName;
            if (setActive)
                _prefab.SetActive(true);
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
            output.name = _prefabName[NamePrefix.Length..];

            return output;
        }

        /// <summary>
        /// Creates a new prefab with the specified name and optionally initialized via <paramref name="initializer"/>.
        /// </summary>
        /// <param name="name">The name of the new prefab.</param>
        /// <param name="initializer">The action to invoke on the new prefab object.</param>
        /// <returns>A new <see cref="FakePrefab"/> with the specified name and optionally initialized using <paramref name="initializer"/>.</returns>
        public static FakePrefab Create(string? name, Action<GameObject>? initializer)
        {
            var tempGO = ObjectFactory.Create("TemporaryPrefabCopy", _prefabContainer);

            initializer?.Invoke(tempGO);

            var output = new FakePrefab(tempGO, name);
            Object.DestroyImmediate(tempGO, true);
            return output;
        }

        /// <summary>
        /// Creates a new prefab using the specified <see cref="FakePrefab"/> as a template.
        /// <br/>
        /// This is basically an easy way to create a prefab that's based off an existing one
        /// without having to first instantiate it, optionally modify the instantiated game object, 
        /// and then create a new prefab with the modified object.
        /// </summary>
        /// <returns>A new <see cref="FakePrefab"/> that is based off the specified prefab and was optionally initialized using <paramref name="initializer"/>.</returns>
        /// <inheritdoc cref="Create(string?, Action{GameObject}?)"/>
        public static FakePrefab CreateCopy(FakePrefab original, string? name, Action<GameObject>? initializer)
        {
            if (original == null)
                throw new ArgumentNullException(nameof(original));

            var tempGO = original.Instantiate(_prefabContainer.transform);
            tempGO.name = "TemporaryPrefabCopy";

            initializer?.Invoke(tempGO);

            var output = new FakePrefab(tempGO, name);
            Object.DestroyImmediate(tempGO, true);
            return output;
        }
    }
}