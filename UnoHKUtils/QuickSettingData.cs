#nullable enable

using System;

namespace UnoHKUtils
{
    [Serializable]
    public readonly struct QuickSettingData
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