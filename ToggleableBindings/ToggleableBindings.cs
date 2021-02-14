#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using GlobalEnums;
using Modding;
using ToggleableBindings.Extensions;
using ToggleableBindings.HKQuickSettings;
using ToggleableBindings.VanillaBindings;
using UnityEngine;
using Vasi;

namespace ToggleableBindings
{
    public sealed partial class ToggleableBindings : Mod, ITogglableMod
    {
        [NotNull, DisallowNull]
        public static ToggleableBindings? Instance { get; private set; }

        [NotNull, DisallowNull]
        public QuickSettings? Settings { get; private set; }

        public ToggleableBindings() : base()
        {
            if (Instance is not null)
                return;

            Instance = this;
        }

        public override void Initialize()
        {
            AddHooks();
            Settings = new();
            BindingManager.Initialize();
        }

        public void Unload()
        {
            RemoveHooks();
            if (Settings.CurrentSaveSlot is not null)
                Settings.SaveSaveSettings();

            Settings.Unload();
            BindingManager.Unload();
        }

        public override string GetVersion()
        {
            return VersionUtil.GetVersion<ToggleableBindings>();
        }
    }
}