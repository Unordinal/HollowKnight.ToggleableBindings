#nullable enable

using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;

namespace UnoHKUtils.PlayMaker.Extensions
{
    public static class FsmEventExtensions
    {
        /// <summary>
        /// Finds the <see cref="FsmState"/> objects that receive the <see cref="FsmEvent"/>. <see cref="FsmEvent.IsGlobal"/> needs to be <see langword="true"/> for other FSMs to receive it.
        /// </summary>
        /// <param name="evnt">The <see cref="FsmEvent"/> to find the receivers of.</param>
        /// <returns>A collection of <see cref="FsmState"/> objects that receive the <see cref="FsmEvent"/>.</returns>
        public static IEnumerable<FsmState> FindReceiverStates(this FsmEvent evnt)
        {
            // TODO: Respect global flag
            var statesList = Fsm.FsmList.Where((fsm) => fsm.Events.Contains(evnt)).SelectMany((fsm) => fsm.States);
            foreach (var state in statesList)
            {
                var findMatch = state.Transitions.Where((transition) => transition.FsmEvent == evnt);
                if (findMatch.Any())
                    yield return state;
            }
        }
    }
}