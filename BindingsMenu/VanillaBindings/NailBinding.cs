using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using TogglableBindings.HKQuickSettings;
using UnityEngine;

namespace TogglableBindings.VanillaBindings
{
    public sealed class NailBinding : Binding
    {
        internal override bool IsVanillaBinding => true;

        [QuickSetting]
        public static int MaxBoundNailDamage { get; private set; } = 13;

        private const string ShowBoundNailEvent = "SHOW BOUND NAIL";
        private const string HideBoundNailEvent = "HIDE BOUND NAIL";
        private readonly List<IDetour> _detours;

        public NailBinding() : base(nameof(NailBinding))
        {
            var boundNailGetter = typeof(BossSequenceController).GetMethod($"get_BoundNail", BindingFlags.Public | BindingFlags.Static);
            var boundNailDamageGetter = typeof(BossSequenceController).GetMethod("get_BoundNailDamage", BindingFlags.Public | BindingFlags.Static);

            _detours = new()
            {
                new Hook(boundNailGetter, new Func<bool>(() => true), ModConstants.HookManualApply),
                new Hook(boundNailDamageGetter, new Func<int>(BoundNailDamageOverride), ModConstants.HookManualApply)
            };
        }

        protected override void OnApplied()
        {
            On.GameManager.FinishedEnteringScene += GameManager_FinishedEnteringScene;
            IL.BossSequenceController.RestoreBindings += BossSequenceController_RestoreBindings;
            foreach (var detour in _detours)
                detour.Apply();

            CoroutineController.Start(OnAppliedCoroutine());
        }

        protected override void OnRestored()
        {
            On.GameManager.FinishedEnteringScene -= GameManager_FinishedEnteringScene;
            IL.BossSequenceController.RestoreBindings -= BossSequenceController_RestoreBindings;
            foreach (var detour in _detours)
                detour.Undo();

            CoroutineController.Start(OnRestoredCoroutine());
        }

        private IEnumerator OnAppliedCoroutine()
        {
            yield return new WaitWhile(() => HeroController.instance is null);
            yield return null;
            yield return new WaitWhile(() => !EventRegister.eventRegister.ContainsKey(ShowBoundNailEvent));

            EventRegister.SendEvent(ShowBoundNailEvent);
        }

        private IEnumerator OnRestoredCoroutine()
        {
            yield return new WaitWhile(() => HeroController.instance is null);
            yield return null;

            EventRegister.SendEvent(HideBoundNailEvent);
        }

        private void GameManager_FinishedEnteringScene(On.GameManager.orig_FinishedEnteringScene orig, GameManager self)
        {
            orig(self);
            CoroutineController.Start(OnAppliedCoroutine());
        }

        private void BossSequenceController_RestoreBindings(ILContext il)
        {
            ILCursor c = new(il);

            c.GotoNext
            (
                i => i.MatchLdstr(HideBoundNailEvent),
                i => i.MatchCall<EventRegister>("SendEvent")
            );
            c.RemoveRange(2);
        }

        private static int BoundNailDamageOverride()
        {
            int nailDamage = GameManager.instance.playerData.nailDamage;

            return (nailDamage >= MaxBoundNailDamage) ? MaxBoundNailDamage : Mathf.RoundToInt(nailDamage * 0.8f);
        }
    }
}