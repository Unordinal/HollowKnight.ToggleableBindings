#nullable enable

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Modding;
using ToggleableBindings.HKQuickSettings;
using ToggleableBindings.UI;
using UnityEngine;
using Vasi;

namespace ToggleableBindings
{
    /// <summary>
    /// The entry point for the <see cref="ToggleableBindings"/> mod.
    /// </summary>
    public sealed partial class ToggleableBindings : Mod
    {
        /// <summary>
        /// Gets the instance of this mod.
        /// </summary>
        [NotNull, DisallowNull]
        public static ToggleableBindings? Instance { get; private set; }

        [QuickSetting, DefaultValue(true)]
        internal static bool EnforceBindingRestrictions { get; private set; } = true;

        [NotNull, DisallowNull]
        internal QuickSettings? Settings { get; private set; }

        private ToggleableBindings() : base()
        {
            if (Instance != null)
            {
                LogError($"An instance of {nameof(ToggleableBindings)} already exists!");
                return;
            }

            Instance = this;
        }

        /// <inheritdoc/>
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            BaseGamePrefabs.Initialize(preloadedObjects);

            AddHooks();
            Settings = new();
            BindingManager.Initialize();
            BindingsUIController.Initialize();

            LogDebug("Initialized.");
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

        /// <inheritdoc/>
        public override int LoadPriority() => -10;

        /// <inheritdoc/>
        public override List<(string, string)> GetPreloadNames()
        {
            return new()
            {
                ("GG_Atrium", "GG_Challenge_Door"),
                ("Room_mapper", "Shop Menu")
            };
        }

        /// <inheritdoc/>
        public override string GetVersion() => VersionUtil.GetVersion<ToggleableBindings>();
    }
}