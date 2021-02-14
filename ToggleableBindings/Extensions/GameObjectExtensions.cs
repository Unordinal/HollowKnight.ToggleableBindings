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
    }
}
