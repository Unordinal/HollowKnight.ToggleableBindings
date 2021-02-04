#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.RuntimeDetour;
using TogglableBindings.HKQuickSettings;
using UnityEngine;

namespace TogglableBindings.VanillaBindings
{
    public class ShellBinding : Binding
    {
        [QuickSetting]
        public static int MaxBoundShellHealth { get; private set; } = 4;

        private const string UpdateBlueHealthEvent = "UPDATE BLUE HEALTH";
        private const string CharmIndicatorCheckEvent = "CHARM INDICATOR CHECK";
        private readonly List<IDetour> _detours;

        public ShellBinding() : base(nameof(ShellBinding))
        {
            var boundShellGetter = typeof(BossSequenceController).GetMethod($"get_BoundShell", BindingFlags.Public | BindingFlags.Static);
            var boundMaxHealthGetter = typeof(BossSequenceController).GetMethod($"get_BoundMaxHealth", BindingFlags.Public | BindingFlags.Static);

            _detours = new(2)
            {
                new Hook(boundShellGetter, new Func<bool>(() => true), ModConstants.HookManualApply),
                new Hook(boundMaxHealthGetter, new Func<int>(BoundMaxHealthOverride), ModConstants.HookManualApply)
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

            PlayerData.instance.MaxHealth();

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