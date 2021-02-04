#nullable enable

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TogglableBindings.HKQuickSettings
{
    /// <summary>
    /// Note: Only works on static members.
    /// <para/>
    /// Specifies that the member with this attribute is a setting that will automatically be saved and loaded to the settings files.
    /// Parameters specify the name and/or whether this setting is save slot-specific.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class QuickSettingAttribute : Attribute
    {
        public string? SettingName { get; }

        public bool IsPerSave { get; }

        public QuickSettingAttribute([CallerMemberName] string? settingName = null, bool isPerSave = false)
        {
            SettingName = settingName;
            IsPerSave = isPerSave;
        }

        public QuickSettingAttribute(bool isPerSave, [CallerMemberName] string? settingName = null) : this(settingName, isPerSave) { }
    }
}