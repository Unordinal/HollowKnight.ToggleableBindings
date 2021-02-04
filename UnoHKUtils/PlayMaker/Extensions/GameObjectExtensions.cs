#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker;
using UnityEngine;
using UnoHKUtils.Extensions;

namespace UnoHKUtils.PlayMaker.Extensions
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Gets the first FSM in the <see cref="GameObject"/> that has the specified <paramref name="fsmName"/>.
        /// </summary>
        /// <param name="fsmName">The name of the FSM to match.</param>
        /// 
        /// <inheritdoc cref="GetFsm(GameObject, Func{PlayMakerFSM, bool}?)"/>
        public static PlayMakerFSM? GetFsm(this GameObject gameObject, string fsmName)
        {
            fsmName.ThrowIfNull(nameof(fsmName));

            return gameObject.GetFsm((fsm) => fsm.FsmName == fsmName);
        }

        /// <summary>
        /// Gets the first FSM in the <see cref="GameObject"/> that optionally matches the specified <paramref name="predicate"/>.
        /// </summary>
        /// <returns>The <see cref="PlayMakerFSM"/> that was matched if one was found; otherwise, <see langword="null"/>.</returns>
        /// 
        /// <inheritdoc cref="GetFsms(GameObject, Func{PlayMakerFSM, bool}?)"/>
        public static PlayMakerFSM? GetFsm(this GameObject gameObject, Func<PlayMakerFSM, bool>? predicate = null)
        {
            return gameObject.GetFsms(predicate).FirstOrDefault();
        }

        /// <summary>
        /// Gets the FSMs in the <see cref="GameObject"/> that optionally match a specified <paramref name="predicate"/>.
        /// </summary>
        /// <param name="gameObject">The game object to get the FSMs of.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <returns>A collection of <see cref="PlayMakerFSM"/> objects.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static IEnumerable<PlayMakerFSM> GetFsms(this GameObject gameObject, Func<PlayMakerFSM, bool>? predicate = null)
        {
            gameObject.ThrowIfNull(nameof(gameObject));

            IEnumerable<PlayMakerFSM> filtered = PlayMakerFSM.FsmList.Where((fsm) => fsm.gameObject == gameObject);
            if (predicate is not null)
                filtered = filtered.Where(predicate);

            return filtered;
        }
    }
}
