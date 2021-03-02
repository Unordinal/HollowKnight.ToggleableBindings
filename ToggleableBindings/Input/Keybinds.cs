#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using InControl;
using ToggleableBindings.HKQuickSettings;

namespace ToggleableBindings.Input
{
    internal static class Keybinds
    {
        private static readonly ActionCombo _defaultOpenBindingsUI;

        [QuickSetting]
        public static ActionCombo OpenBindingsUI { get; private set; }

        static Keybinds()
        {
            var gmi = GameManager.instance;
            if (gmi == null)
                throw new InvalidOperationException("GameManager instance is null!");

            var actions = gmi.inputHandler.inputActions;

            _defaultOpenBindingsUI = new ActionCombo(actions.down, actions.dreamNail);
            if (OpenBindingsUI == null)
                OpenBindingsUI = _defaultOpenBindingsUI;
        }

        // TODO: Refactor. This is a little bit of a mess.
        public static ActionCombo? KeybindStringToCombo(string? keybindStr)
        {
            if (keybindStr == null)
                return null;

            var gmi = GameManager.instance;
            if (gmi == null)
                throw new InvalidOperationException("GameManager instance is null!");

            var heroActions = gmi.inputHandler.inputActions;
            var allActionsNoWS = heroActions.Actions.ToDictionary(a => ReplaceWhiteSpace(a.Name.ToLower()));

            var actionStrs = keybindStr.Split(',').Select(s => s.Trim().ToLower());

            List<PlayerAction> comboActions = new();
            foreach (var actionStr in actionStrs)
            {
                var actionStrNoWS = ReplaceWhiteSpace(actionStr);
                if (allActionsNoWS.TryGetValue(actionStrNoWS, out var playerAction))
                {
                    comboActions.Add(playerAction);
                }
                else
                    ToggleableBindings.Instance.LogError($"Couldn't parse part of keybind string '{actionStr}', ignoring.");
            }

            return new ActionCombo(comboActions);
        }

        public static string ComboToKeybindString(ActionCombo actionCombo)
        {
            var actionStrs = actionCombo.Actions.Select(a => ReplaceWhiteSpace(a.Name)).ToArray();
            return string.Join(",", actionStrs);
        }

        private static string ReplaceWhiteSpace(string value)
        {
            return new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }
    }
}