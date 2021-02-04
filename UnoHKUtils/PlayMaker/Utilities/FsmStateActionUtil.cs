#nullable enable

using System;
using System.Collections;
using HutongGames.PlayMaker;
using UnoHKUtils.PlayMaker.Actions;

namespace UnoHKUtils.PlayMaker.Utilities
{
    public static class FsmStateActionUtil
    {
        /// <param name="method">The method to execute.</param>
        ///
        /// <inheritdoc cref="CreateMethod(Action{Fsm}?)"/>
        public static MethodInvoker CreateMethod(Action method)
        {
            return new((_) => method());
        }

        /// <summary>
        /// Creates a new action that will execute the given <paramref name="method"/>.
        /// </summary>
        /// <param name="method">The method to execute. May be <see langword="null"/>. The first parameter is the <see cref="Fsm"/> of the state the action is attached to.</param>
        /// <returns>A new action which will execute the held method when called.</returns>
        public static MethodInvoker CreateMethod(Action<Fsm>? method)
        {
            return new(method);
        }

        /// <param name="coroutine">The coroutine to execute.</param>
        ///
        /// <inheritdoc cref="CreateCoroutine(Func{Fsm, IEnumerator}?, bool, float, bool)"/>
        public static CoroutineInvoker CreateCoroutine(Func<IEnumerator> coroutine, bool wait, float delay = 0f, bool cancelOnExit = false)
        {
            return CreateCoroutine((_) => coroutine(), wait, delay, cancelOnExit);
        }

        /// <summary>
        /// Creates a new action that will execute the given <paramref name="coroutine"/>.
        /// </summary>
        /// <param name="coroutine">The coroutine to execute. May be <see langword="null"/>. The first paramter is the <see cref="Fsm"/> of the state the action is attached to.</param>
        /// <param name="wait">If <see langword="true"/>, the action will wait until the coroutine is done before sending a finish event to the state.</param>
        /// <param name="delay">If non-zero, waits this many seconds before executing the coroutine.</param>
        /// <param name="cancelOnExit">If <see langword="true"/>, cancels the coroutine when the action is exited.</param>
        /// <returns>A new action which will execute the held coroutine when called.</returns>
        public static CoroutineInvoker CreateCoroutine(Func<Fsm, IEnumerator>? coroutine, bool wait, float delay = 0f, bool cancelOnExit = false)
        {
            return new(coroutine, wait, delay, cancelOnExit);
        }

        /// <param name="coroutine">The coroutine to execute.</param>
        ///
        /// <inheritdoc cref="CreateRepeatingCoroutine(Func{Fsm, IEnumerator?}?, float, float, bool)"/>
        public static RepeatingCoroutineInvoker CreateRepeatingCoroutine(Func<IEnumerator> coroutine, float initialDelay = 0f, float repeatDelay = 1f, bool cancelOnExit = false)
        {
            return CreateRepeatingCoroutine((_) => coroutine(), initialDelay, repeatDelay, cancelOnExit);
        }

        /// <summary>
        /// Creates a new action that will repeatedly execute the given <paramref name="coroutine"/>.
        /// </summary>
        /// <param name="initialDelay"><inheritdoc cref="CreateCoroutine(Func{Fsm, IEnumerator}?, bool, float)" path="//param[3]"/></param>
        /// <param name="repeatDelay">Waits this many seconds between executions of the coroutine.</param>
        /// <returns>A new action which will repeatedly execute the held coroutine when called.</returns>
        ///
        /// <inheritdoc cref="CreateCoroutine(Func{Fsm, IEnumerator}?, bool, float, bool)"/>
        public static RepeatingCoroutineInvoker CreateRepeatingCoroutine(Func<Fsm, IEnumerator>? coroutine, float initialDelay = 0f, float repeatDelay = 1f, bool cancelOnExit = false)
        {
            return new(coroutine, initialDelay, repeatDelay, cancelOnExit);
        }
    }
}