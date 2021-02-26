#nullable enable

using ToggleableBindings.Extensions;
using UnityEngine;

namespace ToggleableBindings.Utility
{
    internal static class ObjectFactory
    {
        private static GameObject? _container;
        private static GameObject? _ddolContainer;

        private static GameObject Container
        {
            get => _container != null
                ? _container
                : _container = CreateInternal($"{nameof(ToggleableBindings)}::{nameof(Container)}", InstanceFlags.Default, false);
        }

        private static GameObject DDOLContainer
        {
            get => _ddolContainer != null
                ? _ddolContainer
                : _ddolContainer = CreateInternal($"{nameof(ToggleableBindings)}::{nameof(DDOLContainer)}", InstanceFlags.DontDestroyOnLoad, false);
        }

        public static GameObject Create(string? name, InstanceFlags instanceFlags = InstanceFlags.Default)
        {
            var output = CreateInternal(name, instanceFlags);

            ToggleableBindings.Instance.LogDebug($"Created new object '{output.name}'");
            return output;
        }

        public static GameObject Instantiate(GameObject original, InstanceFlags instanceFlags = InstanceFlags.Default)
        {
            return Instantiate(original, null, instanceFlags);
        }

        public static GameObject Instantiate(GameObject original, GameObject? parent, InstanceFlags instanceFlags = InstanceFlags.Default)
        {
            if (!original)
                throw new System.ArgumentNullException(nameof(original));

            var clone = Object.Instantiate(original);
            ApplyInstanceFlags(clone, parent, instanceFlags);

            ToggleableBindings.Instance.LogDebug($"Instantiated game object as new object '{clone.name}'");
            return clone;
        }

        public static GameObject Instantiate(FakePrefab prefab, InstanceFlags instanceFlags = InstanceFlags.Default)
        {
            return Instantiate(prefab, null, instanceFlags);
        }

        public static GameObject Instantiate(FakePrefab prefab, GameObject? parent, InstanceFlags instanceFlags = InstanceFlags.Default)
        {
            if (!prefab)
                throw new System.ArgumentNullException(nameof(prefab));

            var instance = prefab.Instantiate();
            ApplyInstanceFlags(instance, parent, instanceFlags);

            ToggleableBindings.Instance.LogDebug($"Instantiated prefab as new object '{instance.name}'");
            return instance;
        }

        private static void ApplyInstanceFlags(GameObject target, GameObject? parent, InstanceFlags instanceFlags, bool useContainer = true)
        {
            bool startInactive = (instanceFlags & InstanceFlags.StartInactive) != 0;
            bool dontDestroyOnLoad = (instanceFlags & InstanceFlags.DontDestroyOnLoad) != 0;
            bool worldPositionDoesNotStay = (instanceFlags & InstanceFlags.WorldPositionDoesNotStay) != 0;

            if (startInactive)
                target.SetActive(false);

            if (dontDestroyOnLoad)
                Object.DontDestroyOnLoad(target);

            if (parent == null && useContainer)
                parent = dontDestroyOnLoad ? DDOLContainer : Container;

            if (parent != null)
                target.SetParent(parent, !worldPositionDoesNotStay);
        }

        private static GameObject CreateInternal(string? name, InstanceFlags instanceFlags = InstanceFlags.Default, bool useContainer = true)
        {
            var output = new GameObject(name);

            ApplyInstanceFlags(output, null, instanceFlags, useContainer);

            return output;
        }
    }
}