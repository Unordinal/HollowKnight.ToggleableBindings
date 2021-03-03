#nullable enable

using UnityEngine;

namespace ToggleableBindings.Utility
{
    /// <summary>
    /// Utility class for working with Unity <see cref="Object"/> types.
    /// </summary>
    public static class ObjectUtil
    {
        private static GameObject? _bench;

        /// <summary>
        /// Checks to see if the Knight is within the specified distance of the given game object.
        /// </summary>
        /// <param name="gameObject">The game object to check the distance for.</param>
        /// <param name="distance">The distance the Knight must be within for this method to return <see langword="true"/>.</param>
        /// <returns><see langword="true"/> if the Knight is within <paramref name="distance"/> of the object; otherwise, <see langword="false"/>.</returns>
        public static bool HeroIsWithinDistanceOf(GameObject gameObject, float distance)
        {
            if (gameObject == null)
                return false;

            if (HeroController.instance == null)
                return false;

            var heroPos = HeroController.instance.transform.position;
            var objPos = gameObject.transform.position;

            return Vector3.Distance(heroPos, objPos) <= distance;
        }

        /// <summary>
        /// Returns the first bench found in the scene or <see langword="null"/> if there isn't a bench.
        /// </summary>
        /// <returns>The first bench found in the scene or <see langword="null"/> if one was not found.</returns>
        public static GameObject? GetBenchInScene()
        {
            if (_bench == null)
            {
                var restBench = Object.FindObjectOfType<RestBench>();
                if (restBench)
                    _bench = restBench.gameObject;
            }

            return _bench;
        }
    }
}