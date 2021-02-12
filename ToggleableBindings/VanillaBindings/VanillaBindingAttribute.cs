using System;

namespace ToggleableBindings.VanillaBindings
{
    /// <summary>
    /// Marks the <see cref="Binding"/> this attribute is defined on as a base game binding.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal class VanillaBindingAttribute : Attribute { }
}