#nullable enable

using UnityEngine;

namespace ToggleableBindings.Utility
{
    public static class ObjectUtil
    {
        private static GameObject? _bench;

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