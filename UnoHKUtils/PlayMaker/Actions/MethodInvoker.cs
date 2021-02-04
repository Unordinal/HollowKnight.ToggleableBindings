#nullable enable

using System;
using HutongGames.PlayMaker;

namespace UnoHKUtils.PlayMaker.Actions
{
    public class MethodInvoker : FsmStateAction
    {
        private readonly Action<Fsm>? _method;

        public MethodInvoker(Action<Fsm>? method)
        {
            _method = method;
        }

        public override void OnEnter()
        {
            _method?.Invoke(Fsm);
            Finish();
        }
    }
}