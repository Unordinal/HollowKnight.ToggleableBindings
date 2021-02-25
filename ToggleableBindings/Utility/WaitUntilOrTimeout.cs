#nullable enable

using System;
using UnityEngine;

namespace ToggleableBindings.Utility
{
    public sealed class WaitUntilOrTimeout : CustomYieldInstruction
    {
        private float _timeout;
        private readonly Func<bool> _condition;

        public override bool keepWaiting => ShouldKeepWaiting();

        public WaitUntilOrTimeout(float timeout, Func<bool> condition)
        {
            _timeout = timeout;
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        private bool ShouldKeepWaiting()
        {
            _timeout -= Time.deltaTime;
            return _timeout > 0f && _condition() == false;
        }
    }
}