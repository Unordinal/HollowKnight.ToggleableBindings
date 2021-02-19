#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.RuntimeDetour;
using ToggleableBindings.HKQuickSettings;
using ToggleableBindings.Utility;
using UnityEngine;

namespace ToggleableBindings.VanillaBindings
{
    [VanillaBinding]
    public sealed class ShellBinding : Binding
    {
        [QuickSetting]
        public static int MaxBoundShellHealth { get; private set; } = 4;

        private const string UpdateBlueHealthEvent = "UPDATE BLUE HEALTH";
        private const string CharmIndicatorCheckEvent = "CHARM INDICATOR CHECK";
        private readonly List<IDetour> _detours;

        private Sprite? _defaultSprite;
        private Sprite? _selectedSprite;

        public override Sprite DefaultSprite => _defaultSprite ??= Prefabs.VanillaShellButton.GetComponent<BossDoorChallengeUIBindingButton>().iconImage.sprite;

        public override Sprite SelectedSprite => _selectedSprite ??= Prefabs.VanillaShellButton.GetComponent<BossDoorChallengeUIBindingButton>().selectedSprite;

        public ShellBinding() : base(nameof(ShellBinding))
        {
            var boundShellGetter = typeof(BossSequenceController).GetMethod($"get_BoundShell", BindingFlags.Public | BindingFlags.Static);
            var boundMaxHealthGetter = typeof(BossSequenceController).GetMethod($"get_BoundMaxHealth", BindingFlags.Public | BindingFlags.Static);

            _detours = new(2)
            {
                new Hook(boundShellGetter, new Func<bool>(() => true), TBConstants.HookManualApply),
                new Hook(boundMaxHealthGetter, new Func<int>(BoundMaxHealthOverride), TBConstants.HookManualApply)
            };
        }

        protected override void OnApplied()
        {
            foreach (var detour in _detours)
                detour.Apply();

            CoroutineController.Start(OnToggledCoroutine());
        }

        protected override void OnRestored()
        {
            foreach (var detour in _detours)
                detour.Undo();

            CoroutineController.Start(OnToggledCoroutine());
        }

        private IEnumerator OnToggledCoroutine()
        {
            yield return new WaitWhile(() => HeroController.instance is null);
            yield return null;

            PlayerData.instance?.MaxHealth();

            PlayMakerFSM.BroadcastEvent(CharmIndicatorCheckEvent);
            EventRegister.SendEvent(UpdateBlueHealthEvent);
        }

        private static int BoundMaxHealthOverride()
        {
            int maxBoundHealth = MaxBoundShellHealth;

            bool fragileHealthEquipped = PlayerData.instance.equippedCharm_23 && !PlayerData.instance.brokenCharm_23;
            if (fragileHealthEquipped)
                maxBoundHealth += 2;

            return maxBoundHealth;
        }
    }
}