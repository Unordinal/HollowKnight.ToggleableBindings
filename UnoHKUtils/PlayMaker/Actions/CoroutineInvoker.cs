#nullable enable

using System;
using System.Collections;
using HutongGames.PlayMaker;
using UnityEngine;

namespace UnoHKUtils.PlayMaker.Actions
{
    public class CoroutineInvoker : FsmStateAction
    {
        protected readonly Func<Fsm, IEnumerator>? _coroutine;
        protected readonly bool _wait;
        protected readonly float _delay;
        protected readonly bool _cancelOnExit;
        protected Coroutine? _currentCoroutine;

        public CoroutineInvoker(Func<Fsm, IEnumerator>? coroutine, bool wait, float delay = 0f, bool cancelOnExit = false)
        {
            _coroutine = coroutine;
            _wait = wait;
            _delay = delay;
            _cancelOnExit = cancelOnExit;
        }

        public override void OnEnter()
        {
            _currentCoroutine = Fsm.Owner.StartCoroutine(Coroutine());
            if (!_wait)
                Finish();
        }

        public override void OnExit()
        {
            if (_cancelOnExit)
                Cancel();
        }

        public virtual void Cancel()
        {
            if (_currentCoroutine is not null)
                Fsm.Owner.StopCoroutine(_currentCoroutine);
        }

        private IEnumerator Coroutine()
        {
            if (_delay > 0)
                yield return new WaitForSeconds(_delay);

            yield return _coroutine?.Invoke(Fsm);

            if (_wait)
                Finish();
        }
    }
}