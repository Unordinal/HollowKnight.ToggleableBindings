using System;

namespace TogglableBindings.HKQuickSettings.JsonNet
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class JsonGetOnlyPropertyAttribute : Attribute { }
}