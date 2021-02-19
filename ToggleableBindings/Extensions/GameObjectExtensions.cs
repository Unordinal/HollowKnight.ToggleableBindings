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

        public static GameObject? FindChildByName(this GameObject gameObject, string childName)
        {
            if (gameObject is null)
                return null;

            foreach (var child in gameObject.GetChildren())
                if (child.name == childName)
                    return child;

            return null;
        }

        public static GameObject FindChildByPath(this GameObject gameObject, string childPath)
        {
            if (gameObject is null)
                throw new ArgumentNullException(nameof(gameObject));

            return gameObject.transform.Find(childPath).gameObject ?? throw new Exception("Couldn't find GameObject via path.");
        }
    }
}
