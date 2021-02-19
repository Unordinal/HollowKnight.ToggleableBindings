#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.RuntimeDetour;
using ToggleableBindings.Utility;
using UnityEngine;

namespace ToggleableBindings.VanillaBindings
{
    [VanillaBinding]
    public sealed class SoulBinding : Binding
    {
        private const string BindVesselOrbEvent = "BIND VESSEL ORB";
        private const string UnbindVesselOrbEvent = "UNBIND VESSEL ORB";
        private const string MPLoseEvent = "MP LOSE";
        private const string MPReserveDownEvent = "MP RESERVE DOWN";
        private readonly List<IDetour> _detours;

        private Sprite? _defaultSprite;
        private Sprite? _selectedSprite;

        public override Sprite DefaultSprite => _defaultSprite ??= Prefabs.VanillaSoulButton.GetComponent<BossDoorChallengeUIBindingButton>().iconImage.sprite;

        public override Sprite SelectedSprite => _selectedSprite ??= Prefabs.VanillaSoulButton.GetComponent<BossDoorChallengeUIBindingButton>().selectedSprite;

        public SoulBinding() : base(nameof(SoulBinding))
        {
            var boundSoulGetter = typeof(BossSequenceController).GetMethod("get_BoundSoul", BindingFlags.Public | BindingFlags.Static);

            _detours = new(1)
            {
                new Hook(boundSoulGetter, new Func<bool>(() => true), TBConstants.HookManualApply)
            };
        }

        protected override void OnApplied()
        {
            On.GGCheckBoundSoul.OnEnter += GGCheckBoundSoul_OnEnter;
            foreach (var detour in _detours)
                detour.Apply();

            CoroutineController.Start(OnAppliedCoroutine());
        }

        protected override void OnRestored()
        {
            On.GGCheckBoundSoul.OnEnter -= GGCheckBoundSoul_OnEnter;
            foreach (var detour in _detours)
                detour.Undo();

            CoroutineController.Start(OnRestoredCoroutine());
        }

        private IEnumerator OnAppliedCoroutine()
        {
            yield return new WaitWhile(() => HeroController.instance is null);
            yield return null;

            int mpLeft = Math.Min(PlayerData.instance.MPCharge, 33);
            PlayerData.instance.ClearMP();
            PlayerData.instance.AddMPCharge(mpLeft);

            var gm = GameManager.instance;
            yield return new WaitWhile(() => gm.soulOrb_fsm is null || gm.soulVessel_fsm is null);
            gm.soulOrb_fsm.SendEvent(MPLoseEvent);
            gm.soulVessel_fsm.SendEvent(MPReserveDownEvent);

            EventRegister.SendEvent(BindVesselOrbEvent);
        }

        private IEnumerator OnRestoredCoroutine()
        {
            yield return new WaitWhile(() => HeroController.instance is null);
            yield return null;

            var gm = GameManager.instance;
            yield return new WaitWhile(() => gm.soulOrb_fsm is null);
            gm.soulOrb_fsm.SendEvent(MPLoseEvent);

            EventRegister.SendEvent(UnbindVesselOrbEvent);
        }

        private void GGCheckBoundSoul_OnEnter(On.GGCheckBoundSoul.orig_OnEnter orig, GGCheckBoundSoul self)
        {
            self.Fsm.Event(self.boundEvent);
            self.Finish();
        }
    }
}