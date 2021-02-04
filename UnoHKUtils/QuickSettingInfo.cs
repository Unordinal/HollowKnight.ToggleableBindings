#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace UnoHKUtils
{
    internal sealed class QuickSettingInfo
    {
        private readonly string? _settingName;

        public MemberInfo MemberInfo { get; init; }

        [AllowNull]
        public string SettingName
        {
            get => _settingName ?? MemberInfo.Name;
            init => _settingName = value;
        }

        public string SettingKey => $"{MemberInfo.DeclaringType.Name}.{SettingName}";

        public bool IsPerSave { get; init; }

        public QuickSettingInfo(MemberInfo memberInfo, string? settingName, bool isPerSave = false)
        {
            MemberInfo = memberInfo;
            SettingName = settingName;
            IsPerSave = isPerSave;
        }
    }
}