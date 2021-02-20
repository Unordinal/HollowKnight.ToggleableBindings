#nullable enable

using ToggleableBindings.Extensions;
using UnityEngine;

namespace ToggleableBindings.Utility
{
    internal static class ObjectFactory
    {
        private static GameObject? _container;
        private static GameObject? _ddolContainer;

        private static GameObject Container => _container ??= new GameObject($"{nameof(ToggleableBindings)}::{nameof(Container)}");

        private static GameObject DDOLContainer
        {
            get
            {
                if (!_ddolContainer)
                {
                    _ddolContainer = new GameObject($"{nameof(ToggleableBindings)}::{nameof(DDOLContainer)}");
                    Object.DontDestroyOnLoad(_ddolContainer);
                }

                return _ddolContainer!;
            }
        }

        public static GameObject Create(string? name, InstanceFlags instanceFlags = InstanceFlags.Default)
        {
            var output = new GameObject(name);
            ApplyInstanceFlags(output, null, instanceFlags);

            ToggleableBindings.Instance.Log("Created new object: " + output.name);
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

            ToggleableBindings.Instance.Log("Instantiated game object as new object: " + clone.name);
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

            ToggleableBindings.Instance.Log("Instantiated prefab as new object: " + instance.name);
            return instance;
        }

        private static void ApplyInstanceFlags(GameObject target, GameObject? parent, InstanceFlags instanceFlags)
        {
            bool startInactive = (instanceFlags & InstanceFlags.StartInactive) != 0;
            bool dontDestroyOnLoad = (instanceFlags & InstanceFlags.DontDestroyOnLoad) != 0;
            bool worldPositionDoesNotStay = (instanceFlags & InstanceFlags.WorldPositionDoesNotStay) != 0;

            target.SetActive(!startInactive);
            if (dontDestroyOnLoad)
                Object.DontDestroyOnLoad(target);

            GameObject container;
            if (parent)
                container = parent!;
            else
                container = dontDestroyOnLoad ? DDOLContainer : Container;

            target.SetParent(container, !worldPositionDoesNotStay);
        }
    }
}