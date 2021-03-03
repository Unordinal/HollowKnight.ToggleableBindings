#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using ToggleableBindings.Utility;
using UnityEngine;

namespace ToggleableBindings.VanillaBindings
{
    [VanillaBinding]
    internal sealed class SoulBinding : Binding
    {
        private const string BindVesselOrbEvent = "BIND VESSEL ORB";
        private const string UnbindVesselOrbEvent = "UNBIND VESSEL ORB";
        private const string MPLoseEvent = "MP LOSE";
        private const string MPReserveDownEvent = "MP RESERVE DOWN";
        private readonly List<IDetour> _detours;

        private Sprite? _defaultSprite;
        private Sprite? _selectedSprite;

        public override Sprite DefaultSprite => _defaultSprite = _defaultSprite != null ? _defaultSprite : _defaultSprite = BaseGamePrefabs.SoulButton.UnsafeGameObject.GetComponent<BossDoorChallengeUIBindingButton>().iconImage.sprite;

        public override Sprite SelectedSprite => _selectedSprite = _selectedSprite != null ? _selectedSprite : _selectedSprite = BaseGamePrefabs.SoulButton.UnsafeGameObject.GetComponent<BossDoorChallengeUIBindingButton>().selectedSprite;

        public SoulBinding() : base("Soul")
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
            IL.BossSequenceController.RestoreBindings += BossSequenceController_RestoreBindings;
            foreach (var detour in _detours)
                detour.Apply();

            CoroutineController.Start(OnAppliedCoroutine());
        }

        private IEnumerator OnAppliedCoroutine()
        {
            yield return new WaitWhile(() => !HeroController.instance);
            yield return null;

            int mpLeft = Math.Min(PlayerData.instance.MPCharge, 33);
            PlayerData.instance.ClearMP();
            PlayerData.instance.AddMPCharge(mpLeft);

            var gm = GameManager.instance;
            yield return new WaitWhile(() => !gm.soulOrb_fsm || !gm.soulVessel_fsm);
            gm.soulOrb_fsm.SendEvent(MPLoseEvent);
            gm.soulVessel_fsm.SendEvent(MPReserveDownEvent);

            EventRegister.SendEvent(BindVesselOrbEvent);
        }

        protected override void OnRestored()
        {
            On.GGCheckBoundSoul.OnEnter -= GGCheckBoundSoul_OnEnter;
            IL.BossSequenceController.RestoreBindings -= BossSequenceController_RestoreBindings;
            foreach (var detour in _detours)
                detour.Undo();

            CoroutineController.Start(OnRestoredCoroutine());
        }

        private IEnumerator OnRestoredCoroutine()
        {
            yield return new WaitWhile(() => !HeroController.instance);
            yield return null;

            var gm = GameManager.instance;
            yield return new WaitWhile(() => !gm.soulOrb_fsm);

            gm.soulOrb_fsm.SendEvent(MPLoseEvent);
            EventRegister.SendEvent(UnbindVesselOrbEvent);
        }

        private void BossSequenceController_RestoreBindings(ILContext il)
        {
            ILCursor c = new(il);

            c.GotoNext
            (
                i => i.MatchLdstr(UnbindVesselOrbEvent),
                i => i.MatchCall(typeof(EventRegister), nameof(EventRegister.SendEvent))
            );
            c.RemoveRange(2);
        }

        private void GGCheckBoundSoul_OnEnter(On.GGCheckBoundSoul.orig_OnEnter orig, GGCheckBoundSoul self)
        {
            self.Fsm.Event(self.boundEvent);
            self.Finish();
        }
    }
}