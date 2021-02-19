#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Newtonsoft.Json;
using ToggleableBindings.HKQuickSettings;
using ToggleableBindings.Utility;
using UnityEngine;
using Vasi;

namespace ToggleableBindings.VanillaBindings
{
    [VanillaBinding]
    public sealed class CharmsBinding : Binding
    {
        [QuickSetting]
        public static bool AllowEssentialCharms { get; private set; } = true;

        [QuickSetting("EssentialCharms")]
        internal static int[] ExemptCharms =
        {
            2, // Compass for testing
            36, // Void Heart
            40, // Grimmchild
        };

        private static PlayMakerFSM CharmsMenuFsm
        {
            get
            {
                var charmsPaneGO = GameObject.FindWithTag("Charms Pane");
                return PlayMakerUtils.FindFsmOnGameObject(charmsPaneGO, "UI Charms");
            }
        }

        private const string ShowBoundCharmsEvent = "SHOW BOUND CHARMS";
        private const string HideBoundCharmsEvent = "HIDE BOUND CHARMS";
        private const string CharmEquipCheckEvent = "CHARM EQUIP CHECK";
        private const string CharmIndicatorCheckEvent = "CHARM INDICATOR CHECK";

        private readonly List<IDetour> _detours;

        [JsonProperty(PropertyName = "PreviouslyEquippedCharms")]
        private List<int> _previousEquippedCharms = new();

        private bool _wasOvercharmed;

        private Sprite? _defaultSprite;
        private Sprite? _selectedSprite;

        public override Sprite DefaultSprite => _defaultSprite ??= Prefabs.VanillaCharmsButton.GetComponent<BossDoorChallengeUIBindingButton>().iconImage.sprite;

        public override Sprite SelectedSprite => _selectedSprite ??= Prefabs.VanillaCharmsButton.GetComponent<BossDoorChallengeUIBindingButton>().selectedSprite;

        public CharmsBinding() : base(nameof(CharmsBinding))
        {
            var boundCharmsGetter = typeof(BossSequenceController).GetMethod($"get_BoundCharms", BindingFlags.Public | BindingFlags.Static);
            _detours = new(1)
            {
                new Hook(boundCharmsGetter, new Func<bool>(() => true), TBConstants.HookManualApply)
            };

            CoroutineBuilder.New
                .WithYield(new WaitWhile(() => CharmsMenuFsm is null))
                .WithAction(SetAllowedCharms)
                .Start();
        }

        protected override void OnApplied()
        {
            IL.BossSequenceController.ApplyBindings += BossSequenceController_ApplyBindings;
            IL.BossSequenceController.RestoreBindings += BossSequenceController_RestoreBindings;
            foreach (var detour in _detours)
                detour.Apply();

            CoroutineController.Start(OnAppliedCoroutine());
        }

        protected override void OnRestored()
        {
            IL.BossSequenceController.ApplyBindings -= BossSequenceController_ApplyBindings;
            IL.BossSequenceController.RestoreBindings -= BossSequenceController_RestoreBindings;
            foreach (var detour in _detours)
                detour.Undo();

            CoroutineController.Start(OnRestoredCoroutine());
        }

        private IEnumerator OnAppliedCoroutine()
        {
            yield return new WaitWhile(() => HeroController.instance is null);

            var equippedCharms = PlayerData.instance.equippedCharms;
            if (!WasApplied)
            {
                _previousEquippedCharms = equippedCharms;
                _wasOvercharmed = PlayerData.instance.overcharmed;
            }
            WasApplied = false;

            // Creates a lookup where the 'true' key is an enumerable of charms that are exempt, and the 'false' key are the ones that aren't.
            // If AllowEssentialCharms is false, then no charms are exempt.
            var charmsAllowed = equippedCharms.ToLookup(id => AllowEssentialCharms && ExemptCharms.Contains(id));

            PlayerData.instance.equippedCharms = charmsAllowed[true].ToList();
            PlayerData.instance.overcharmed = false;
            ToggleCharms(charmsAllowed[false], false);

            PlayerData.instance.CalculateNotchesUsed();
            HeroController.instance.CharmUpdate();
            PlayMakerFSM.BroadcastEvent(CharmEquipCheckEvent);
            PlayMakerFSM.BroadcastEvent(CharmIndicatorCheckEvent);

            yield return new WaitWhile(() => !EventRegister.eventRegister.ContainsKey(ShowBoundCharmsEvent));
            EventRegister.SendEvent(ShowBoundCharmsEvent);

            CharmsMenuFsm?.SetState("Activate UI"); // TODO: Test
        }

        private IEnumerator OnRestoredCoroutine()
        {
            yield return null;

            ToggleCharms(PlayerData.instance.equippedCharms.ToArray(), false);

            PlayerData.instance.equippedCharms = _previousEquippedCharms.ToList();
            PlayerData.instance.overcharmed = _wasOvercharmed;

            ToggleCharms(_previousEquippedCharms, true);

            PlayerData.instance.CalculateNotchesUsed();
            HeroController.instance.CharmUpdate();
            PlayMakerFSM.BroadcastEvent(CharmEquipCheckEvent);
            PlayMakerFSM.BroadcastEvent(CharmIndicatorCheckEvent);

            EventRegister.SendEvent(HideBoundCharmsEvent);

            CharmsMenuFsm?.SetState("Activate UI"); // TODO: Test

            _previousEquippedCharms.Clear();
            _wasOvercharmed = false;
        }

        private void SetAllowedCharms()
        {
            if (CharmsMenuFsm is not null)
            {
                var deactivateUI = CharmsMenuFsm.GetState("Deactivate UI");
                for (int i = 0; i < deactivateUI.Actions.Length; i++)
                {
                    ref var curr = ref deactivateUI.Actions[i];
                    if (curr is GGCheckBoundCharms checkCharmsAction)
                    {
                        var replacementAction = new CheckBoundAndCharmIsExempt
                        {
                            Fsm = checkCharmsAction.Fsm,
                            Owner = checkCharmsAction.Owner,
                            State = checkCharmsAction.State
                        };

                        replacementAction.FalseEvent = checkCharmsAction.trueEvent;
                        replacementAction.Instance = this;
                        curr = replacementAction;
                        return;
                    }
                    else if (curr is CheckBoundAndCharmIsExempt cbcie)
                    {
                        cbcie.Instance = this;
                        return;
                    }
                }
            }
        }

        private void ToggleCharms(IEnumerable<int> charmIDs, bool state)
        {
            foreach (var charmID in charmIDs)
                PlayerData.instance.SetBool($"equippedCharm_{charmID}", state);
        }

        private void BossSequenceController_ApplyBindings(ILContext il)
        {
            ILCursor c = new(il);

            c.GotoNext
            (
                MoveType.AfterLabel,
                i => i.MatchCallOrCallvirt(typeof(BossSequenceController), "get_BoundCharms"),
                i => i.MatchBrfalse(out _)
            );

            c.Emit(OpCodes.Ldc_I4_0).Remove();
        }

        private void BossSequenceController_RestoreBindings(ILContext il)
        {
            ILCursor c = new(il);

            c.GotoNext
            (
                MoveType.AfterLabel,
                i => i.MatchCallOrCallvirt(typeof(BossSequenceController), "get_BoundCharms"),
                i => i.MatchBrfalse(out _)
            );

            c.Emit(OpCodes.Ldc_I4_0).Remove();
        }

        private class CheckBoundAndCharmIsExempt : FsmStateAction
        {
            public FsmEvent? TrueEvent;
            public FsmEvent? FalseEvent;

            public CharmsBinding? Instance;

            public override void OnEnter()
            {
                DoCheckForNecessaryCharms();
                Finish();
            }

            public override void Reset()
            {
                TrueEvent = null;
                FalseEvent = null;
            }

            private object? DoCheckForNecessaryCharms()
            {
                if (Instance?.IsApplied == true)
                {
                    if (AllowEssentialCharms)
                    {
                        var charm = Fsm.GetFsmInt("Current Item Number");
                        if (charm is not null && ExemptCharms.Contains(charm.Value))
                            return DoEvent(TrueEvent);
                    }
                }
                else
                {
                    if (!BossSequenceController.BoundCharms)
                        return DoEvent(TrueEvent);
                }

                return DoEvent(FalseEvent);
            }

            // Simply because I like not including braces on my 'if's.
            private object? DoEvent(FsmEvent? fsmEvent)
            {
                Fsm.Event(fsmEvent);
                return null;
            }
        }
    }
}