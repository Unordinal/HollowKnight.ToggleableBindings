#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Modding;
using ToggleableBindings.Extensions;
using ToggleableBindings.HKQuickSettings;
using ToggleableBindings.UI;
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

        public override List<(string, string)> GetPreloadNames()
        {
            return new()
            {
                ("GG_Atrium", "GG_Challenge_Door")
            };
        }

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

            //On.BossDoorChallengeUIBindingButton.OnPointerClick += (orig, self, eventData) => { };

            /*var test = Object.Instantiate(Prefabs.BindingsUI);
            Object.DontDestroyOnLoad(test);
            test.name = nameof(UI.BindingsUI);*/
        }

        public void Unload()
        {
            RemoveHooks();
            if (Settings.CurrentSaveSlot != null)
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