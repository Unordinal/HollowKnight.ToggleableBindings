#nullable enable

using System;
using UnityEngine;

namespace ToggleableBindings.Utility
{
    /// <summary>
    /// Waits until either a condition becomes <see langword="true"/> or the specified amount of time passes.
    /// </summary>
    public sealed class WaitUntilOrTimeout : CustomYieldInstruction
    {
        private float _timeout;
        private readonly Func<bool> _condition;

        /// <inheritdoc/>
        public override bool keepWaiting => ShouldKeepWaiting();

        /// <summary>
        /// Suspends the coroutine execution until either the supplied delegate becomes <see langword="true"/> or the specified amount of seconds passes.
        /// </summary>
        /// <param name="timeout">The timeout, in seconds. When this amount of time passes, coroutine execution is resumed.</param>
        /// <param name="condition">The delegate to check. When this delegate returns <see langword="true"/> or <paramref name="timeout"/> passes, coroutine execution is continued.</param>
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