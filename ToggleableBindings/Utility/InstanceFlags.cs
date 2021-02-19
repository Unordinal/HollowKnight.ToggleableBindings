using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToggleableBindings.Utility
{
    /// <summary>
    /// Flags that specify settings for instantiating an object.
    /// </summary>
    [Flags]
    internal enum InstanceFlags
    {
        /// <summary>
        /// Specifies no instantiation flags.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Call <see cref="UObject.DontDestroyOnLoad(UObject)"/> on the resulting instance.
        /// </summary>
        DontDestroyOnLoad = 1 << 0,

        /// <summary>
        /// Starts the instantiated object in an inactive state.
        /// </summary>
        StartInactive = 1 << 2,

        /// <summary>
        /// If a parent is specified, the instantiated object should not keep its world position.
        /// </summary>
        WorldPositionDoesNotStay = 1 << 3
    }
}
