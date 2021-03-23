#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using GlobalEnums;
using InControl;
using ToggleableBindings.HKQuickSettings;
using ToggleableBindings.Utility;

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

            _defaultOpenBindingsUI = new ActionCombo(HeroActionButton.DOWN, HeroActionButton.SUPER_DASH);

            if (OpenBindingsUI == null)
                OpenBindingsUI = _defaultOpenBindingsUI;
        }

        public static ActionCombo? KeybindStringToCombo(string? keybindStr)
        {
            if (StringUtil.IsNullOrWhiteSpace(keybindStr))
                return null;

            var gmi = GameManager.instance;
            if (gmi == null)
                throw new InvalidOperationException("GameManager instance is null!");

            var heroActions = gmi.inputHandler.inputActions;
            var allActionsNoWS = heroActions.Actions.ToDictionary(a => StringUtil.RemoveWhiteSpace(a.Name.ToLower()));

            var actionStrs = keybindStr.Split(',').Select(s => s.Trim().ToLower());

            List<HeroActionButton> comboActions = new();
            foreach (var actionStr in actionStrs)
            {
                var actionStrNoWS = StringUtil.RemoveWhiteSpace(actionStr);
                if (allActionsNoWS.TryGetValue(actionStrNoWS, out var playerAction))
                {
                    var actionName = playerAction.Name;
                    var actionNameCleaned = StringUtil.RemoveWhiteSpace(actionName).ToLower();
                    comboActions.Add(GetActionButtonForString(actionNameCleaned));
                }
                else
                    ToggleableBindings.Instance.LogError($"Couldn't parse part of keybind string '{actionStr}', ignoring.");
            }

            return new ActionCombo(comboActions);
        }

        public static string ComboToKeybindString(ActionCombo actionCombo)
        {
            var actionStrs = actionCombo.Actions.Select(actionButton => ActionButtonToString[actionButton]);
            return StringUtil.Join(',', actionStrs);
        }

        public static Dictionary<string, HeroActionButton> StringToActionButton = new()
        {
            { "up",         HeroActionButton.UP },
            { "down",       HeroActionButton.DOWN },
            { "left",       HeroActionButton.LEFT },
            { "right",      HeroActionButton.RIGHT },
            { "jump",       HeroActionButton.JUMP },
            { "attack",     HeroActionButton.ATTACK },
            { "cast",       HeroActionButton.CAST },
            { "quickcast",  HeroActionButton.QUICK_CAST },
            { "dash",       HeroActionButton.DASH },
            { "superdash",  HeroActionButton.SUPER_DASH },
            { "quickmap",   HeroActionButton.QUICK_MAP },
            { "inventory",  HeroActionButton.INVENTORY },
            { "dreamnail",  HeroActionButton.DREAM_NAIL },
        };

        public static Dictionary<HeroActionButton, string> ActionButtonToString = StringToActionButton.ToDictionary(kv => kv.Value, kv => kv.Key);

        public static HeroActionButton GetActionButtonForString(string action)
        {
            if (StringToActionButton.TryGetValue(action, out var value))
                return value;

            throw new ArgumentException($"No HeroActionButton matched the action value of '{action}'.");
        }
    }
}