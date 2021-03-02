#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using InControl;
using Newtonsoft.Json;
using ToggleableBindings.Collections;
using ToggleableBindings.JsonNet;

namespace ToggleableBindings.Input
{
    [JsonConverter(typeof(ActionComboJsonConverter))]
    internal class ActionCombo
    {
        private readonly List<PlayerAction> _actions = new();

        public IReadOnlyList<PlayerAction> Actions { get; }

        public bool IsPressed => _actions.All(a => a.IsPressed);

        public bool WasPressed => _actions.All(a => a.WasPressed);

        public bool WasReleased => _actions.All(a => a.WasReleased);

        public ActionCombo(params PlayerAction[] actions) : this(actions.AsEnumerable()) { }

        public ActionCombo(IEnumerable<PlayerAction> actions)
        {
            if (actions == null)
                throw new ArgumentNullException(nameof(actions));

            _actions.AddRange(actions);
            Actions = new ReadOnlyList<PlayerAction>(_actions);
        }

        public bool IsSameAs(ActionCombo other)
        {
            if (other == null)
                return false;

            return Actions.SequenceEqual(other.Actions);
        }
    }
}