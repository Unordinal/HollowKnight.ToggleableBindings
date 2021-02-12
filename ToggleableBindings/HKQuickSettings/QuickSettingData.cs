#nullable enable

using System;

namespace ToggleableBindings.HKQuickSettings
{
    [Serializable]
    internal readonly struct QuickSettingData
    {
        public string SettingName { get; init; }

        public object? SettingValue { get; init; }

        public QuickSettingData(string key, object? value)
        {
            SettingName = key;
            SettingValue = value;
        }
    }
}