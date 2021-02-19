#nullable enable

using ToggleableBindings.Extensions;
using UnityEngine;

namespace ToggleableBindings.Utility
{
    internal static class ObjectFactory
    {
        private static GameObject? _container;
        private static GameObject? _ddolContainer;

        public static GameObject Container => _container ??= new GameObject($"{nameof(ToggleableBindings)}::{nameof(Container)}");

        public static GameObject DDOLContainer
        {
            get
            {
                if (_ddolContainer is null)
                {
                    _ddolContainer = new GameObject($"{nameof(ToggleableBindings)}::{nameof(DDOLContainer)}");
                    Object.DontDestroyOnLoad(_ddolContainer);
                }

                return _ddolContainer;
            }
        }

        public static GameObject Create(string? name, InstanceFlags instanceFlags = InstanceFlags.Default)
        {
            var output = new GameObject(name);
            FollowInstanceFlags(output, null, instanceFlags);

            return output;
        }

        public static GameObject Instantiate(GameObject original, InstanceFlags instanceFlags = InstanceFlags.Default)
        {
            return Instantiate(original, null, instanceFlags);
        }

        public static GameObject Instantiate(GameObject original, GameObject? parent, InstanceFlags instanceFlags = InstanceFlags.Default)
        {
            if (original is null)
                throw new System.ArgumentNullException(nameof(original));

            var clone = Object.Instantiate(original);
            FollowInstanceFlags(clone, parent, instanceFlags);

            return clone;
        }

        private static void FollowInstanceFlags(GameObject gameObject, GameObject? parent, InstanceFlags instanceFlags)
        {
            bool startInactive = (instanceFlags & InstanceFlags.StartInactive) != 0;
            bool dontDestroyOnLoad = (instanceFlags & InstanceFlags.DontDestroyOnLoad) != 0;
            bool worldPositionDoesNotStay = (instanceFlags & InstanceFlags.WorldPositionDoesNotStay) != 0;

            gameObject.SetActive(!startInactive);
            if (dontDestroyOnLoad)
                Object.DontDestroyOnLoad(gameObject);

            var container = parent ?? (dontDestroyOnLoad ? DDOLContainer : Container);
            gameObject.SetParent(container, !worldPositionDoesNotStay);
        }
    }
}