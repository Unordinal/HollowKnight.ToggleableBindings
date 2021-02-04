using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker;
using UnityEngine;
using UnoHKUtils.Collections;
using UnoHKUtils.Extensions;
using UnoHKUtils.FsmUtils;

namespace UnoHKUtils
{
    public static class FsmUtil
    {
        internal const int IndentBy = 4;

        public static IEnumerable<TreeNode<FsmGOPair>> CreateFsmTree()
        {
            List<Fsm> fsmList = Fsm.FsmList;
            var fsmPairs = fsmList.Select(fsm => new TreeNode<FsmGOPair>(new FsmGOPair { Fsm = fsm, GameObject = fsm.GameObject })).ToList();
            fsmPairs.ForEach(n =>
            {
                n.Children.Clear();
                foreach (var child in fsmPairs.Where(p => p.Value.GameObject?.GetParent() == n.Value.GameObject))
                    n.Children.Add(child);
            });

            foreach (var pair in fsmPairs.ToArray())
            {
                TreeNode<FsmGOPair> childNode = pair;
                GameObject parentGO = childNode.Value.GameObject;
                while ((parentGO = parentGO?.GetParent()) is not null)
                {
                    var newPair = new FsmGOPair { GameObject = parentGO, Fsm = parentGO.GetComponent<PlayMakerFSM>()?.Fsm };
                    TreeNode<FsmGOPair> parentNode = new TreeNode<FsmGOPair>(newPair);
                    childNode.Parent = parentNode;
                    childNode = parentNode;

                    fsmPairs.Add(parentNode);
                }
            }

            var finalPairs = 
                fsmPairs.Where(n => n.Value.Fsm is not null)
                .DistinctBy(n => new { n.Value.Fsm, n.Value.GameObject })
                .DistinctBy(n => n.Value.GameObject);

            IEnumerable<TreeNode<FsmGOPair>> rootNodes = finalPairs.Where(n => n.Parent is null);
            /*foreach (var node in rootNodes)
                Log(node.Value.ToString(), 0);*/

            return rootNodes;
        }

        public static void PrintAllFsms()
        {
            var fsmList = PlayMakerFSM.FsmList;
            foreach (var pmFsm in fsmList.Distinct())
            {
                PrintPlayMakerFsm(pmFsm);
                LogEmpty();
            }
        }

        public static void PrintPlayMakerFsm(PlayMakerFSM pmFsm, int indent = 0)
        {
            Log($"[PlayMaker FSM: {pmFsm.FsmName}]", indent);
            PrintFsm(pmFsm.Fsm, indent);
        }

        public static void PrintFsm(Fsm fsm, int indent)
        {
            Log($"FSM: '{fsm.Name}'", indent);
            Log($"Description: {fsm.Description}", indent);
            if (fsm.Template is not null)
                Log($"Template: {fsm.Template.name} - {fsm.Template.Category}", indent);
            indent += IndentBy;
            if (fsm.GameObject is not null)
            {
                Log("Parent GO:", indent);
                PrintUnityGO(fsm.GameObject, indent + IndentBy);
            }
            if (fsm.ActiveState is not null)
            {
                Log($"Active State: '{fsm.ActiveState.Name}'", indent);
            }

            if (fsm.Events.Any())
            {
                Log("Events:", indent);
                PrintEvents(fsm.Events, indent + IndentBy);
            }
            if (fsm.GlobalTransitions.Any())
            {
                Log("Global Transitions:", indent);
                foreach (var transition in fsm.GlobalTransitions)
                    PrintTransition(transition, indent + IndentBy);
            }
            if (fsm.States.Any())
            {
                Log("States:", indent);
                foreach (var state in fsm.States)
                    PrintState(state, indent + IndentBy);
            }
            LogEmpty();
        }

        public static void PrintState(FsmState state, int indent)
        {
            Log($"State '{state.Name}'", indent);
            indent += IndentBy;
            Log($"Description: {state.Description}", indent);
            Log($"Loop Count: {state.loopCount}/{state.maxLoopCount}", indent);
            if (state.ActiveAction is not null)
            {
                Log($"Active Action: {state.ActiveAction.Name}", indent);
            }
            if (state.Transitions.Any())
            {
                Log("Transitions:", indent);
                foreach (var transition in state.Transitions.Where(t => t.FsmEvent?.IsGlobal == true))
                    PrintTransition(transition, indent + IndentBy);
                foreach (var transition in state.Transitions.Where(t => t.FsmEvent?.IsGlobal != true))
                    PrintTransition(transition, indent + IndentBy);
            }
            if (state.Actions.Any())
            {
                Log($"Actions ({state.ActionData.ActionCount}):", indent);
                foreach (var action in state.Actions)
                    PrintAction(action, indent + IndentBy);
            }
            LogEmpty();
        }

        public static void PrintTransition(FsmTransition transition, int indent)
        {
            string output = "On ";
            if (transition.FsmEvent?.IsGlobal == true)
                output += "global ";
            output += $"event '{transition.EventName}', go to state '{transition.ToState}'";

            Log(output, indent);
        }

        public static void PrintAction(FsmStateAction action, int indent)
        {
            action.LogInfo(indent);
            /*string output = $"Action '{action.Name}' ({action.GetType()?.Name})";
            List<(string, object)> props = new();

            if (!string.IsNullOrEmpty(action.AutoName()))
                props.Add((nameof(action.AutoName), action.AutoName()));

            string statuses = null;
            if (action.Active)
                statuses += nameof(action.Active) + " ";
            if (action.Finished)
                statuses += nameof(action.Finished) + " ";
            if (action.IsOpen)
                statuses += nameof(action.IsOpen) + " ";
            if (action.Entered)
                statuses += nameof(action.Entered) + " ";
            props.Add(("Status(es)", statuses));

            if (props.Any())
                output += " " + PropListToString(props);

            Log(output, indent);
            var fields = MiscUtil.GetAllFieldsOfType<INameable>(action);
            if (fields.Any())
                Log("Fields:", indent);
            indent += IndentBy;
            foreach (var (Name, Tooltip, Value) in fields)
            {
                Log($"{Name} - {Tooltip}", indent);
                Log($"{Value}", indent);
            }*/
        }

        public static void PrintEvents(IEnumerable<FsmEvent> events, int indent, bool sortByGlobal = true)
        {
            if (sortByGlobal)
            {
                foreach (var evnt in events.Where(e => e.IsGlobal))
                    PrintEvent(evnt, indent);
                foreach (var evnt in events.Where(e => !e.IsGlobal))
                    PrintEvent(evnt, indent);
            }
            else
            {
                foreach (var evnt in events)
                    PrintEvent(evnt, indent);
            }
        }

        public static void PrintEvent(FsmEvent evnt, int indent)
        {
            string output = $"Event '{evnt.Name}'";
            if (evnt.IsGlobal)
                output = "Global " + output;

            List<(string, object)> propList = new();

            if (!string.IsNullOrEmpty(evnt.Path))
                propList.Add((nameof(evnt.Path), evnt.Path));

            output += " " + PropListToString(propList);
            Log(output, indent);
        }

        public static void PrintUnityGO(GameObject go, int indent)
        {
            string output = $"'{go}'";
            List<string> hierarchy = new() { go.name };
            GameObject parent = go;
            while ((parent = parent.GetParent()) is not null)
                hierarchy.Add(parent.name);

            output += " (Hierarchy: " + string.Join(" -> ", hierarchy.ToArray()) + ")";
            Log(output, indent);
        }

        internal static void Log(object message, int indent)
        {
            UnoHKUtils.Instance.Log(new string(' ', indent) + message.ToString());
        }

        internal static void LogEmpty()
        {
            Log("", 0);
        }

        private static string PropListToString(List<(string Name, object Value)> propList)
        {
            if (!propList.Any())
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append('(');
            for (int i = 0; i < propList.Count; i++)
            {
                var (Name, Value) = propList[i];
                sb.Append($"{Name}: {Value}");
                if (i != propList.Count - 1)
                    sb.Append(", ");
            }
            sb.Append(')');

            return sb.ToString();
        }

        private static GameObject GetParent(this GameObject gameObject)
        {
            return gameObject?.transform?.parent?.gameObject;
        }

        private static GameObject GetRoot(this GameObject gameObject)
        {
            return gameObject?.transform?.root?.gameObject;
        }
    }
}
