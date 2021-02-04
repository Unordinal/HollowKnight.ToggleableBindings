#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker;
using Modding;
using MonoMod.RuntimeDetour;
using UnityEngine;
using Vasi;

namespace UnoHKUtils
{
    public class UnoHKUtils : Mod, ITogglableMod
    {
        private static List<PlayMakerFSM> _fsmList = PlayMakerFSM.FsmList.ToList();

        public static UnoHKUtils Instance { get; private set; } = null!; // Will be set on initialize.

        public static void LogObj(object obj)
        {
            Instance.Log(obj);
        }

        public override string GetVersion() => VersionUtil.GetVersion<UnoHKUtils>();

        public override void Initialize()
        {
            Instance = this;
            /*On.GameManager.FinishedEnteringScene += GameManager_FinishedEnteringScene;
            // On.HutongGames.PlayMaker.Fsm.Start et. al.
            On.PlayMakerFixedUpdate.FixedUpdate += PlayMakerFixedUpdate_FixedUpdate;
            On.CharmDisplay.Start += CharmDisplay_Start;*/
        }

        public void Unload()
        {
        }

        private void CharmDisplay_Start(On.CharmDisplay.orig_Start orig, CharmDisplay self)
        {
            GameObject gameObject = GameObject.FindWithTag("Charms Pane");
            PlayMakerFSM charmsMenuFsm;
            if (gameObject)
            {
                charmsMenuFsm = PlayMakerFSM.FindFsmOnGameObject(gameObject, "UI Charms");
                if (charmsMenuFsm is not null)
                {
                    var deactiveUI = charmsMenuFsm.GetState("Deactivate UI");
                    for (int i = 0; i < deactiveUI.Actions.Length; i++)
                    {
                        var curr = deactiveUI.Actions[i];
                        if (curr is GGCheckBoundCharms ggCBC)
                        {
                            var replacement = new CheckForNecessaryCharms();
                            /*replacement.Fsm = curr.Fsm;
                            replacement.Owner = curr.Owner;
                            replacement.State = curr.State;
                            replacement.Active = curr.Active;
                            replacement.Enabled = curr.Enabled;
                            replacement.Entered = curr.Entered;
                            replacement.IsOpen = curr.IsOpen;
                            replacement.Finished = curr.Finished;
                            replacement.trueEvent = null;*/
                            replacement.falseEvent = ggCBC.trueEvent;
                            deactiveUI.Actions[i] = replacement;
                        }
                    }
                }
            }
            orig(self);
        }

        private void PlayMakerFixedUpdate_FixedUpdate(On.PlayMakerFixedUpdate.orig_FixedUpdate orig, PlayMakerFixedUpdate self)
        {
            orig(self);
            if (!PlayMakerFSM.FsmList.SequenceEqual(_fsmList))
            {
                _fsmList = PlayMakerFSM.FsmList.ToList();
                foreach (var pmFsm in PlayMakerFSM.FsmList)
                    Log(pmFsm.name + " : " + pmFsm.FsmName);
            }
        }

        private void GameManager_FinishedEnteringScene(On.GameManager.orig_FinishedEnteringScene orig, GameManager self)
        {
            orig(self);

            foreach (var pmFsm in PlayMakerFSM.FsmList)
                Log(pmFsm.name + " : " + pmFsm.FsmName);
            /*var tree = FsmUtil.CreateFsmTree();
            foreach (var node in tree)
            {
                int indent = 4;
                Log(new string('-', 30));
                Log(node.Value.ToString());
                Log(new string('-', 30));
                foreach (var child in node.Children)
                    child.Traverse((p, d) => FsmUtil.Log(p.Value.ToString(), indent * d));
                Log(null);
                Log(null);
            }
            foreach (var pair in Fsm.FsmList.Select(fsm => new FsmGOPair { Fsm = fsm, GameObject = fsm.GameObject }))
                Log(pair);*/
        }

        public class CheckForNecessaryCharms : FsmStateAction
        {
            private static readonly int[] ExemptCharms =
            {
                2, // Compass for testing
                36, // Void Heart
                40, // Grimmchild
            };

            public FsmEvent? trueEvent;
            public FsmEvent? falseEvent;

            public override void OnEnter()
            {
                DoCheckForNecessaryCharms();
                Finish();
            }

            public override void OnUpdate()
            {
                DoCheckForNecessaryCharms();
            }

            public override void Reset()
            {
                trueEvent = null;
                falseEvent = null;
            }

            private void DoCheckForNecessaryCharms()
            {
                if (!BossSequenceController.BoundCharms)
                    Fsm.Event(trueEvent);

                var charm = Fsm.GetFsmInt("Current Item Number");
                if (charm is not null)
                {
                    if (ExemptCharms.Contains(charm.Value))
                        Fsm.Event(trueEvent);
                    else
                        Fsm.Event(falseEvent);
                }
            }
        }
    }
}