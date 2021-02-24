#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Modding;
using ToggleableBindings.HKQuickSettings;
using ToggleableBindings.UI;
using UnityEngine;
using Vasi;

namespace ToggleableBindings
{
    public sealed partial class ToggleableBindings : Mod
    {
        internal static event Action? Unloading;

        [NotNull, DisallowNull]
        public static ToggleableBindings? Instance { get; private set; }

        [NotNull, DisallowNull]
        internal QuickSettings? Settings { get; private set; }

        public override List<(string, string)> GetPreloadNames()
        {
            return new()
            {
                ("GG_Atrium", "GG_Challenge_Door"),
                ("Room_mapper", "Shop Menu")
            };
        }

        public override int LoadPriority() => -10;

        public ToggleableBindings() : base()
        {
            if (Instance != null)
                return;

            Instance = this;
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            BaseGamePrefabs.Initialize(preloadedObjects);

            AddHooks();
            Settings = new();
            BindingManager.Initialize();
            BindingsUIController.Initialize();
        }

        /*public void Unload()
        {
            RemoveHooks();
            if (Settings.CurrentSaveSlot != null)
                Settings.SaveSaveSettings();

            Settings.Unload();
            BindingManager.Unload();
            BindingsUIController.Unload();

            Unloading?.Invoke();
        }*/

        public override string GetVersion()
        {
            return VersionUtil.GetVersion<ToggleableBindings>();
        }
    }
}