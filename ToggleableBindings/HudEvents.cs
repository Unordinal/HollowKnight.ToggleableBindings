#nullable enable

using HutongGames.PlayMaker;
using System;
using System.Collections;
using UnityEngine;
using Vasi;

namespace ToggleableBindings
{
    internal static class HudEvents
    {
        public static event Action? ComeIn;
        public static event Action? In;
        public static event Action? GoOut;
        public static event Action? Out;

        static HudEvents()
        {
            GameManager.instance.StartCoroutine(Initialize());
        }

        private static IEnumerator Initialize()
        {
            yield return new WaitWhile(() => (GameCameras.instance != null ? GameCameras.instance.hudCanvas : null) == null);

            var hudCanvas = GameCameras.instance.hudCanvas;
            Fsm slideOut = null!;
            yield return new WaitWhile(() => (slideOut = PlayMakerFSM.FindFsmOnGameObject(hudCanvas, "Slide Out")?.Fsm!) == null);

            var stateComeIn = slideOut.GetState("Come In");
            stateComeIn.AddMethod(() => ComeIn?.Invoke());

            var stateIn = slideOut.GetState("In");
            stateIn.AddMethod(() => In?.Invoke());

            var stateGoOut = slideOut.GetState("Go Out");
            stateGoOut.AddMethod(() => GoOut?.Invoke());

            var stateOut = slideOut.GetState("Out");
            stateOut.AddMethod(() => Out?.Invoke());

            /*var inAction = new CallStaticMethod
            {
                Fsm = stateIn.Fsm,
                Owner = stateIn.Fsm.Owner.gameObject,
                State = stateIn,
                className = typeof(HudEvents).FullName,
                methodName = nameof(OnHudIn),
                parameters = new FsmVar[0],
                storeResult = new FsmVar()
            };*/
        }
    }
}