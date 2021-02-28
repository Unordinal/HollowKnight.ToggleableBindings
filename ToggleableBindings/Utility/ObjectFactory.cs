#nullable enable

using System;
using ToggleableBindings.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

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
                : _container = CreateInternal($"{nameof(ToggleableBindings)}::{nameof(Container)}", null, InstanceFlags.Default, false);
        }

        private static GameObject DDOLContainer
        {
            get => _ddolContainer != null
                ? _ddolContainer
                : _ddolContainer = CreateInternal($"{nameof(ToggleableBindings)}::{nameof(DDOLContainer)}", null, InstanceFlags.DontDestroyOnLoad, false);
        }

        public static GameObject Create(string? name = null, GameObject? parent = null, InstanceFlags instanceFlags = InstanceFlags.Default)
        {
            var output = CreateInternal(name, parent, instanceFlags);
            return output;
        }

        public static GameObject Instantiate(GameObject original, InstanceFlags instanceFlags = InstanceFlags.Default)
        {
            return Instantiate(original, null, instanceFlags);
        }

        public static GameObject Instantiate(GameObject original, GameObject? parent, InstanceFlags instanceFlags = InstanceFlags.Default)
        {
            if (!original)
                throw new ArgumentNullException(nameof(original));

            var clone = Object.Instantiate(original);
            ApplyInstanceFlags(clone, parent, instanceFlags);

            return clone;
        }

        public static GameObject Instantiate(FakePrefab prefab, InstanceFlags instanceFlags = InstanceFlags.Default)
        {
            return Instantiate(prefab, null, instanceFlags);
        }

        public static GameObject Instantiate(FakePrefab prefab, GameObject? parent, InstanceFlags instanceFlags = InstanceFlags.Default)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            var instance = prefab.Instantiate();
            ApplyInstanceFlags(instance, parent, instanceFlags);

            return instance;
        }

        private static void ApplyInstanceFlags(GameObject target, GameObject? parent, InstanceFlags instanceFlags, bool useContainer = true)
        {
            bool startInactive = (instanceFlags & InstanceFlags.StartInactive) != 0;
            bool dontDestroyOnLoad = (instanceFlags & InstanceFlags.DontDestroyOnLoad) != 0;
            bool worldPositionStays = (instanceFlags & InstanceFlags.WorldPositionStays) != 0;

            if (startInactive)
                target.SetActive(false);

            if (dontDestroyOnLoad)
                Object.DontDestroyOnLoad(target);

            if (parent == null && useContainer)
                parent = dontDestroyOnLoad ? DDOLContainer : Container;

            if (parent != null)
                target.SetParent(parent, worldPositionStays);
        }

        private static GameObject CreateInternal(string? name, GameObject? parent = null, InstanceFlags instanceFlags = InstanceFlags.Default, bool useContainer = true)
        {
            var output = new GameObject(name);

            ApplyInstanceFlags(output, parent, instanceFlags, useContainer);

            return output;
        }
    }
}