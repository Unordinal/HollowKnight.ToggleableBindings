using System;

namespace ToggleableBindings.HKQuickSettings.JsonNet
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class JsonGetOnlyPropertyAttribute : Attribute { }
}