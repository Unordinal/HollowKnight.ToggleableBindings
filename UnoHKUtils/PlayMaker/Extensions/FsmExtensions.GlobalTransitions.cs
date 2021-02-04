#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using UnoHKUtils.Extensions;

namespace UnoHKUtils.PlayMaker.Extensions
{
    public static partial class FsmExtensions
    {
        // Extensions for Fsm with FsmTransition (Global)

        #region Get Methods

        /// <summary>
        /// Gets the transition in the <see cref="Fsm"/> that has the specified <paramref name="eventName"/>.
        /// </summary>
        /// <param name="eventName">The event name of the transition to get.</param>
        ///
        /// <inheritdoc cref="GetGlobalTransition(Fsm, Func{FsmTransition, bool}?)"/>
        public static FsmTransition? GetGlobalTransition(this Fsm fsm, string eventName)
        {
            eventName.ThrowIfNull(nameof(eventName));

            return fsm.GetGlobalTransition((transition) => transition.EventName == eventName);
        }

        /// <summary>
        /// Gets the transition in the <see cref="Fsm"/> at the specified <paramref name="transitionIndex"/>.
        /// </summary>
        /// <param name="transitionIndex">The index of the transition to get.</param>
        /// <returns>The transition retrieved from the specified index.</returns>
        ///
        /// <inheritdoc cref="GetGlobalTransition(Fsm, Func{FsmTransition, bool}?)"/>
        /// <exception cref="IndexOutOfRangeException"/>
        public static FsmTransition GetGlobalTransition(this Fsm fsm, int transitionIndex)
        {
            fsm.ThrowIfNull(nameof(fsm));

            return fsm.GlobalTransitions[transitionIndex];
        }

        /// <summary>
        /// Gets the first transition in the <see cref="Fsm"/> that optionally matches a specified <paramref name="predicate"/>.
        /// </summary>
        /// <returns>The <see cref="FsmTransition"/> that was matched if one was found; otherwise, <see langword="null"/>.</returns>
        ///
        /// <inheritdoc cref="GetGlobalTransitions(Fsm, Func{FsmTransition, bool}?)"/>
        public static FsmTransition? GetGlobalTransition(this Fsm fsm, Func<FsmTransition, bool>? predicate = null)
        {
            return fsm.GetGlobalTransitions(predicate).FirstOrDefault();
        }

        /// <summary>
        /// Gets the transitions in the <see cref="Fsm"/> that optionally match a specified <paramref name="predicate"/>.
        /// </summary>
        /// <param name="fsm">The FSM to use.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <returns>A collection of <see cref="FsmTransition"/> objects.</returns>
        public static IEnumerable<FsmTransition> GetGlobalTransitions(this Fsm fsm, Func<FsmTransition, bool>? predicate = null)
        {
            fsm.ThrowIfNull(nameof(fsm));

            return (predicate is null) ? fsm.GlobalTransitions.AsEnumerable() : fsm.GlobalTransitions.Where(predicate);
        }

        #endregion

        #region Replace Methods

        /// <summary>
        /// Replaces the transition in the <see cref="Fsm"/> that has the specified <paramref name="eventName"/> with the given <paramref name="replacementTransition"/>.
        /// </summary>
        /// <param name="eventName">The event name of the transition to replace.</param>
        ///
        /// <inheritdoc cref="ReplaceGlobalTransition(Fsm, Func{FsmTransition, bool}, FsmTransition)"/>
        public static bool ReplaceGlobalTransition(this Fsm fsm, string eventName, FsmTransition replacementTransition)
        {
            eventName.ThrowIfNull(nameof(eventName));

            return fsm.ReplaceGlobalTransition((transition) => transition.EventName == eventName, replacementTransition);
        }

        /// <summary>
        /// Replaces the transition at the specified <paramref name="transitionIndex"/> in the <see cref="Fsm"/> with the given <paramref name="replacementTransition"/>.
        /// </summary>
        /// <param name="transitionIndex">The index of the transition in the <see cref="Fsm"/> to replace.</param>
        /// <returns/>
        ///
        /// <inheritdoc cref="ReplaceGlobalTransition(Fsm, Func{FsmTransition, bool}, FsmTransition)"/>
        /// <exception cref="IndexOutOfRangeException"/>
        public static void ReplaceGlobalTransition(this Fsm fsm, int transitionIndex, FsmTransition replacementTransition)
        {
            fsm.ThrowIfNull(nameof(fsm));
            replacementTransition.ThrowIfNull(nameof(replacementTransition));

            fsm.GlobalTransitions[transitionIndex] = replacementTransition;
        }

        /// <summary>
        /// Replaces the first transition in the <see cref="Fsm"/> that is equal to <paramref name="targetTransition"/> with the given <paramref name="replacementTransition"/>.
        /// </summary>
        /// <param name="targetTransition">The transition in the <see cref="Fsm"/> to replace.</param>
        ///
        /// <inheritdoc cref="ReplaceGlobalTransition(Fsm, Func{FsmTransition, bool}, FsmTransition)"/>
        public static bool ReplaceGlobalTransition(this Fsm fsm, FsmTransition targetTransition, FsmTransition replacementTransition)
        {
            targetTransition.ThrowIfNull(nameof(targetTransition));

            return fsm.ReplaceGlobalTransition((transition) => transition == targetTransition, replacementTransition);
        }

        /// <summary>
        /// Replaces the first transition in the <see cref="Fsm"/> that matches the specified predicate with
        /// the given <paramref name="replacementTransition"/>.
        /// </summary>
        /// <param name="fsm">The FSM to look through.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <param name="replacementTransition">The transition to replace the found transition with.</param>
        /// <returns><see langword="true"/> if an item was successfully found and replaced; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static bool ReplaceGlobalTransition(this Fsm fsm, Func<FsmTransition, bool> predicate, FsmTransition replacementTransition)
        {
            fsm.ThrowIfNull(nameof(fsm));
            predicate.ThrowIfNull(nameof(predicate));
            replacementTransition.ThrowIfNull(nameof(replacementTransition));

            int targetIndex = Array.FindIndex(fsm.GlobalTransitions, (transition) => predicate(transition));
            if (targetIndex is -1)
                return false;

            fsm.GlobalTransitions[targetIndex] = replacementTransition;
            return true;
        }

        #endregion

        #region Add Methods

        /// <summary>
        /// Adds the specified transition to the <see cref="Fsm"/>.
        /// </summary>
        /// <param name="fsm">The FSM to add to.</param>
        /// <param name="transition">The transition to add.</param>
        public static void AddGlobalTransition(this Fsm fsm, FsmTransition transition)
        {
            fsm.ThrowIfNull(nameof(fsm));
            transition.ThrowIfNull(nameof(transition));

            fsm.GlobalTransitions = fsm.GlobalTransitions.Add(transition);
        }

        /// <summary>
        /// Adds the specified states to the <see cref="Fsm"/>.
        /// </summary>
        /// <param name="fsm">The FSM to add to.</param>
        /// <param name="transitions">The transition to add.</param>
        public static void AddGlobalTransitions(this Fsm fsm, params FsmTransition[] transitions)
        {
            fsm.ThrowIfNull(nameof(fsm));
            transitions.ThrowIfNull(nameof(transitions));

            fsm.GlobalTransitions = fsm.GlobalTransitions.AddRange(transitions);
        }

        #endregion

        #region Insert Methods

        /// <summary>
        /// Inserts the specified transition into the <see cref="Fsm"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="transition">The transition to insert.</param>
        ///
        /// <inheritdoc cref="InsertGlobalTransitions(Fsm, int, FsmTransition[])"/>
        public static void InsertGlobalTransition(this Fsm fsm, int index, FsmTransition transition)
        {
            fsm.ThrowIfNull(nameof(fsm));
            transition.ThrowIfNull(nameof(transition));

            fsm.GlobalTransitions = fsm.GlobalTransitions.Insert(index, transition);
        }

        /// <summary>
        /// Inserts the specified transitions into the <see cref="Fsm"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="fsm">The FSM to insert into.</param>
        /// <param name="index">The index in the <see cref="Fsm"/> to insert at.</param>
        /// <param name="transitions">The transitions to insert.</param>
        public static void InsertGlobalTransitions(this Fsm fsm, int index, params FsmTransition[] transitions)
        {
            fsm.ThrowIfNull(nameof(fsm));
            transitions.ThrowIfNull(nameof(transitions));

            fsm.GlobalTransitions = fsm.GlobalTransitions.InsertRange(index, transitions);
        }

        #endregion

        #region Remove Methods

        /// <summary>
        /// Removes the first transition with the specified <paramref name="eventName"/> from the <see cref="Fsm"/>.
        /// </summary>
        /// <param name="eventName">The event name of the transition to remove.</param>
        ///
        /// <inheritdoc cref="RemoveGlobalTransitions(Fsm, Func{FsmTransition, bool})"/>
        public static bool RemoveGlobalTransition(this Fsm fsm, string eventName)
        {
            return fsm.RemoveGlobalTransition((transition) => transition.EventName == eventName);
        }

        /// <summary>
        /// Removes the transition at the specified <paramref name="transitionIndex"/> from the <see cref="Fsm"/>.
        /// </summary>
        /// <param name="transitionIndex">The index of the transition to remove.</param>
        /// <returns/>
        ///
        /// <inheritdoc cref="RemoveGlobalTransitions(Fsm, Func{FsmTransition, bool})"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static void RemoveGlobalTransition(this Fsm fsm, int transitionIndex)
        {
            fsm.ThrowIfNull(nameof(fsm));

            fsm.GlobalTransitions = fsm.GlobalTransitions.RemoveAt(transitionIndex);
        }

        /// <summary>
        /// Removes the specified transition from the <see cref="Fsm"/>.
        /// </summary>
        /// <param name="targetTransition">The transition to remove.</param>
        ///
        /// <inheritdoc cref="RemoveGlobalTransitions(Fsm, Func{FsmTransition, bool})"/>
        public static bool RemoveGlobalTransition(this Fsm fsm, FsmTransition targetTransition)
        {
            targetTransition.ThrowIfNull(nameof(targetTransition));

            return fsm.RemoveGlobalTransition((transition) => transition == targetTransition);
        }

        /// <summary>
        /// Removes the first transition that matches the specified <paramref name="predicate"/> from the <see cref="Fsm"/>.
        /// </summary>
        ///
        /// <inheritdoc cref="RemoveGlobalTransitions(Fsm, Func{FsmTransition, bool})"/>
        public static bool RemoveGlobalTransition(this Fsm fsm, Func<FsmTransition, bool> predicate)
        {
            fsm.ThrowIfNull(nameof(fsm));
            predicate.ThrowIfNull(nameof(predicate));

            var withoutTransition = fsm.GlobalTransitions.WithoutFirst(predicate).ToArray();
            bool wasRemoved = withoutTransition.Length != fsm.GlobalTransitions.Length;

            fsm.GlobalTransitions = withoutTransition;
            return wasRemoved;
        }

        /// <summary>
        /// Removes the specified transitions from the <see cref="Fsm"/>.
        /// </summary>
        /// <param name="targetTransitions">The transitions to remove.</param>
        ///
        /// <inheritdoc cref="RemoveGlobalTransitions(Fsm, Func{FsmTransition, bool})"/>
        public static bool RemoveGlobalTransitions(this Fsm fsm, params FsmTransition[] targetTransitions)
        {
            targetTransitions.ThrowIfNull(nameof(targetTransitions));

            return fsm.RemoveGlobalTransitions((transition) => targetTransitions.Contains(transition));
        }

        /// <summary>
        /// Removes the transitions from the <see cref="Fsm"/> that match the specified <paramref name="predicate"/>.
        /// </summary>
        /// <param name="fsm">The FSM to remove from.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <returns><see langword="true"/> if an item was successfully found and removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static bool RemoveGlobalTransitions(this Fsm fsm, Func<FsmTransition, bool> predicate)
        {
            fsm.ThrowIfNull(nameof(fsm));
            predicate.ThrowIfNull(nameof(predicate));

            var withoutTransitions = fsm.GlobalTransitions.Where((transition) => !predicate(transition)).ToArray();
            bool anyRemoved = withoutTransitions.Length != fsm.GlobalTransitions.Length;

            fsm.GlobalTransitions = withoutTransitions;
            return anyRemoved;
        }

        #endregion
    }
}