#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using ToggleableBindings.HKQuickSettings;
using ToggleableBindings.Utility;
using UnityEngine;

namespace ToggleableBindings.VanillaBindings
{
    [VanillaBinding]
    internal sealed class NailBinding : Binding
    {
        [QuickSetting]
        public static int MaxBoundNailDamage { get; private set; } = 13;

        private const string ShowBoundNailEvent = "SHOW BOUND NAIL";
        private const string HideBoundNailEvent = "HIDE BOUND NAIL";
        private readonly List<IDetour> _detours;

        private Sprite? _defaultSprite;
        private Sprite? _selectedSprite;

        public override Sprite DefaultSprite => _defaultSprite = _defaultSprite != null ? _defaultSprite : _defaultSprite = BaseGamePrefabs.NailButton.UnsafeGameObject.GetComponent<BossDoorChallengeUIBindingButton>().iconImage.sprite;

        public override Sprite SelectedSprite => _selectedSprite = _selectedSprite != null ? _selectedSprite : _selectedSprite = BaseGamePrefabs.NailButton.UnsafeGameObject.GetComponent<BossDoorChallengeUIBindingButton>().selectedSprite;

        public NailBinding() : base("Nail")
        {
            var boundNailGetter = typeof(BossSequenceController).GetMethod($"get_BoundNail", BindingFlags.Public | BindingFlags.Static);
            var boundNailDamageGetter = typeof(BossSequenceController).GetMethod("get_BoundNailDamage", BindingFlags.Public | BindingFlags.Static);

            _detours = new(2)
            {
                new Hook(boundNailGetter, new Func<bool>(() => true), TBConstants.HookManualApply),
                new Hook(boundNailDamageGetter, new Func<int>(BoundNailDamageOverride), TBConstants.HookManualApply)
            };
        }

        protected override void OnApplied()
        {
            IL.BossSequenceController.RestoreBindings += BossSequenceController_RestoreBindings;
            foreach (var detour in _detours)
                detour.Apply();

            CoroutineController.Start(OnAppliedCoroutine());
        }

        private IEnumerator OnAppliedCoroutine()
        {
            yield return new WaitWhile(() => !HeroController.instance);
            yield return new WaitWhile(() => !EventRegister.eventRegister.ContainsKey(ShowBoundNailEvent));

            EventRegister.SendEvent(ShowBoundNailEvent);
        }

        protected override void OnRestored()
        {
            IL.BossSequenceController.RestoreBindings -= BossSequenceController_RestoreBindings;
            foreach (var detour in _detours)
                detour.Undo();

            EventRegister.SendEvent(HideBoundNailEvent);
        }

        private void BossSequenceController_RestoreBindings(ILContext il)
        {
            ILCursor c = new(il);

            c.GotoNext(
                i => i.MatchLdstr(HideBoundNailEvent),
                i => i.MatchCall<EventRegister>(nameof(EventRegister.SendEvent))
            );
            c.RemoveRange(2);
        }

        private static int BoundNailDamageOverride()
        {
            int nailDamage = GameManager.instance.playerData.nailDamage;

            return (nailDamage > MaxBoundNailDamage) ? MaxBoundNailDamage : Mathf.RoundToInt(nailDamage * 0.8f);
        }
    }
}