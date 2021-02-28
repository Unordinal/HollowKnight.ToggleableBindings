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

        public static PlayMakerFSM? CharmsMenuFsm
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

        public override Sprite DefaultSprite => _defaultSprite = _defaultSprite != null ? _defaultSprite : _defaultSprite = BaseGamePrefabs.CharmsButton.UnsafeGameObject.GetComponent<BossDoorChallengeUIBindingButton>().iconImage.sprite;

        public override Sprite SelectedSprite => _selectedSprite = _selectedSprite != null ? _selectedSprite : _selectedSprite = BaseGamePrefabs.CharmsButton.UnsafeGameObject.GetComponent<BossDoorChallengeUIBindingButton>().selectedSprite;

        public CharmsBinding() : base("Charms")
        {
            var boundCharmsGetter = typeof(BossSequenceController).GetMethod($"get_BoundCharms", BindingFlags.Public | BindingFlags.Static);
            _detours = new(1)
            {
                new Hook(boundCharmsGetter, new Func<bool>(() => true), TBConstants.HookManualApply)
            };

            ToggleableBindings.Instance.LogDebug("CharmsBinding Ctor");
        }

        protected override void OnApplied()
        {
            IL.BossSequenceController.ApplyBindings += BossSequenceController_ApplyBindings;
            IL.BossSequenceController.RestoreBindings += BossSequenceController_RestoreBindings;
            foreach (var detour in _detours)
                detour.Apply();

            CoroutineController.Start(OnAppliedCoroutine());
            CoroutineController.Start(SetAllowedCharms());
        }

        private IEnumerator OnAppliedCoroutine()
        {
            yield return new WaitWhile(() => !HeroController.instance);

            var equippedCharms = PlayerData.instance.equippedCharms;
            _previousEquippedCharms = equippedCharms;
            _wasOvercharmed = PlayerData.instance.overcharmed;

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

            if (CharmsMenuFsm != null)
                CharmsMenuFsm.SetState("Activate UI");
        }

        protected override void OnRestored()
        {
            IL.BossSequenceController.ApplyBindings -= BossSequenceController_ApplyBindings;
            IL.BossSequenceController.RestoreBindings -= BossSequenceController_RestoreBindings;
            foreach (var detour in _detours)
                detour.Undo();

            ToggleCharms(PlayerData.instance.equippedCharms.ToArray(), false);

            PlayerData.instance.equippedCharms = _previousEquippedCharms.ToList();
            PlayerData.instance.overcharmed = _wasOvercharmed;

            ToggleCharms(_previousEquippedCharms, true);

            PlayerData.instance.CalculateNotchesUsed();
            HeroController.instance.CharmUpdate();
            PlayMakerFSM.BroadcastEvent(CharmEquipCheckEvent);
            PlayMakerFSM.BroadcastEvent(CharmIndicatorCheckEvent);

            EventRegister.SendEvent(HideBoundCharmsEvent);

            if (CharmsMenuFsm != null)
                CharmsMenuFsm.SetState("Activate UI");
        }

        private IEnumerator SetAllowedCharms()
        {
            yield return new WaitWhile(() => CharmsMenuFsm == null);

            var stateDeactivateUI = CharmsMenuFsm.GetState("Deactivate UI");
            var stateActions = stateDeactivateUI.Actions;
            for (int i = 0; i < stateActions.Length; i++)
            {
                var curr = stateActions[i];
                if (curr is GGCheckBoundCharms checkBoundCharms)
                {
                    var replacement = new CheckCanEquipCharm
                    {
                        Fsm = checkBoundCharms.Fsm,
                        Owner = checkBoundCharms.Owner,
                        State = checkBoundCharms.State,
                        FalseEvent = checkBoundCharms.trueEvent,
                        Instance = this
                    };

                    stateActions[i] = replacement;
                    yield break;
                }
                else if (curr is CheckCanEquipCharm checkBoundAndCharm)
                {
                    checkBoundAndCharm.Instance = this;
                    yield break;
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

        private class CheckCanEquipCharm : FsmStateAction
        {
            public FsmEvent? TrueEvent;
            public FsmEvent? FalseEvent;

            public CharmsBinding? Instance;

            public override void OnEnter()
            {
                DoCheckCanEquipCharm();
                Finish();
            }

            public override void Reset()
            {
                TrueEvent = null;
                FalseEvent = null;
            }

            private void DoCheckCanEquipCharm()
            {
                FsmEvent? resultEvent = FalseEvent;

                if (Instance?.IsApplied == true)
                {
                    if (AllowEssentialCharms)
                    {
                        FsmInt charmID = Fsm.GetFsmInt("Current Item Number");
                        if (charmID != null && ExemptCharms.Contains(charmID.Value))
                            resultEvent = TrueEvent;
                    }
                }
                else
                {
                    if (!BossSequenceController.BoundCharms)
                        resultEvent = TrueEvent;
                }

                Fsm.Event(resultEvent);
            }
        }
    }
}