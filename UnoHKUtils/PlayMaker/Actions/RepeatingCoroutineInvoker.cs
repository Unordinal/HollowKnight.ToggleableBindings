#nullable enable

using System;
using System.Collections;
using HutongGames.PlayMaker;
using UnityEngine;

namespace UnoHKUtils.PlayMaker.Actions
{
    public class RepeatingCoroutineInvoker : CoroutineInvoker
    {
        protected readonly float _repeatDelay;
        protected bool _repeating;

        public RepeatingCoroutineInvoker(Func<Fsm, IEnumerator>? coroutine, float initialDelay = 0f, float repeatDelay = 1f, bool cancelOnExit = false)
            : base(coroutine, false, initialDelay, cancelOnExit)
        {
            _repeating = true;
            _repeatDelay = repeatDelay;
        }

        public override void OnEnter()
        {
            _currentCoroutine = Fsm.Owner.StartCoroutine(Coroutine());
            Finish();
        }

        /// <summary>
        /// Cancels the coroutine.
        /// </summary>
        public override void Cancel()
        {
            _repeating = false;
            if (_currentCoroutine is not null)
                Fsm.Owner.StopCoroutine(_currentCoroutine);
        }

        private IEnumerator Coroutine()
        {
            if (_delay > 0)
                yield return new WaitForSeconds(_delay);

            do
            {
                yield return _coroutine?.Invoke(Fsm);

                if (_repeating && _repeatDelay > 0)
                    yield return new WaitForSeconds(_repeatDelay);
                else
                    yield return null;
            }
            while (_repeating);
        }
    }
}