#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using GlobalEnums;
using InControl;
using Newtonsoft.Json;
using ToggleableBindings.JsonNet;

namespace ToggleableBindings.Input
{
    [JsonConverter(typeof(ActionComboJsonConverter))]
    internal class ActionCombo
    {
        private readonly List<HeroActionButton> _actions = new();

        public IReadOnlyList<HeroActionButton> Actions { get; }

        public bool IsPressed => _actions.All(a => ButtonToPlayerAction(a).IsPressed);

        public bool WasPressed => _actions.All(a => ButtonToPlayerAction(a).WasPressed);

        public bool WasReleased => _actions.All(a => ButtonToPlayerAction(a).WasReleased);

        public ActionCombo(params HeroActionButton[] actions) : this(actions.AsEnumerable()) { }

        public ActionCombo(IEnumerable<HeroActionButton> actions)
        {
            if (actions == null)
                throw new ArgumentNullException(nameof(actions));

            _actions.AddRange(actions);
            Actions = new List<HeroActionButton>(_actions);
        }

        public bool IsSameAs(ActionCombo other)
        {
            if (other == null)
                return false;

            return Actions.SequenceEqual(other.Actions);
        }

        private static PlayerAction ButtonToPlayerAction(HeroActionButton heroActionButton)
        {
            return GameManager.instance.inputHandler.ActionButtonToPlayerAction(heroActionButton);
        }
    }
}