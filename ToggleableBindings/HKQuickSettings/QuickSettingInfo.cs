#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ToggleableBindings.HKQuickSettings
{
    public sealed class QuickSettingInfo
    {
        private readonly string? _name;

        /// <summary>
        /// Gets the setting's <see cref="System.Reflection.MemberInfo"/>. This
        /// is the field or property that this setting gets and sets the value of.
        /// </summary>
        public MemberInfo MemberInfo { get; init; }

        /// <summary>
        /// Gets the setting's name. If <see langword="null"/>, returns <see cref="MemberInfo.Name"/>.
        /// <para/>
        /// Example: '<c>MySpecialVariable</c>'
        /// </summary>
        [AllowNull]
        public string Name
        {
            get => _name ?? MemberInfo.Name;
            init => _name = value;
        }

        /// <summary>
        /// Gets the setting's key. This is the declaring type name of the
        /// setting (the type that holds the <see cref="System.Reflection.MemberInfo"/>)
        /// plus a period, then <see cref="Name"/>.
        /// <para/>
        /// Example: '<c>MyDeclaringType.MySpecialVariable</c>'
        /// </summary>
        public string Key => $"{MemberInfo.DeclaringType.Name}.{Name}";

        /// <summary>
        /// Gets whether this setting is save slot-specific - that is, saved and
        /// loaded when the game starts and ends or saved and loaded along with a specific save slot.
        /// </summary>
        public bool IsPerSave { get; init; }

        public QuickSettingInfo(MemberInfo memberInfo, string? settingName, bool isPerSave = false)
        {
            MemberInfo = memberInfo;
            Name = settingName;
            IsPerSave = isPerSave;
        }
    }
}