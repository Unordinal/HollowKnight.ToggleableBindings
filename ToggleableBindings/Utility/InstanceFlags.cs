using UnityEngine;

namespace ToggleableBindings.Utility
{
    /// <summary>
    /// Flags that specify settings for instantiating an object.
    /// </summary>
    [System.Flags]
    internal enum InstanceFlags
    {
        /// <summary>
        /// Specifies no instantiation flags.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Call <see cref="Object.DontDestroyOnLoad(Object)"/> on the resulting instance.
        /// </summary>
        DontDestroyOnLoad = 1 << 0,

        /// <summary>
        /// Starts the instantiated object in an inactive state.
        /// </summary>
        StartInactive = 1 << 2,

        /// <summary>
        /// If a parent is specified, the instantiated object should keep its world position.
        /// </summary>
        WorldPositionStays = 1 << 3
    }
}