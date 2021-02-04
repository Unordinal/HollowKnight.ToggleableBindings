#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker;

namespace UnoHKUtils.PlayMaker.Extensions
{
    public static class FsmTransitionExtensions
    {
        /// <summary>
        /// Finds the parent <see cref="FsmState"/> of the <see cref="FsmTransition"/>.
        /// </summary>
        /// <param name="transition">The object to find the parent of.</param>
        /// <returns>An <see cref="FsmState"/> that is the parent of <paramref name="transition"/> if one was found; otherwise, <see langword="null"/>.</returns>
        public static FsmState? FindParent(this FsmTransition transition)
        {
            var statesList = Fsm.FsmList.SelectMany((fsm) => fsm.States);
            return statesList.Where((state) => state.Transitions.Contains(transition)).FirstOrDefault();
        }
    }
}
