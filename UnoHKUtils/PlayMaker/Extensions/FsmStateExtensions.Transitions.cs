#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using UnoHKUtils.Extensions;

namespace UnoHKUtils.PlayMaker.Extensions
{
    public static partial class FsmStateExtensions
    {
        // Extensions for FsmState with FsmTransition

        #region Get Methods

        /// <summary>
        /// Gets the transition in the <see cref="FsmState"/> with the specified event name.
        /// </summary>
        /// <param name="eventName">The transition's event name to look for.</param>
        ///
        /// <inheritdoc cref="GetTransition(FsmState, Func{FsmTransition, bool})"/>
        public static FsmTransition? GetTransition(this FsmState state, string eventName)
        {
            return state.GetTransition((transition) => transition.EventName == eventName);
        }

        /// <summary>
        /// Gets the transition at the specified <paramref name="transitionIndex"/> in the <see cref="FsmState"/>.
        /// </summary>
        /// <param name="transitionIndex">The index of the transition to retrieve.</param>
        /// <returns>The <see cref="FsmTransition"/> in the state at the specified index.</returns>
        ///
        /// <inheritdoc cref="GetTransition(FsmState, string)"/>
        /// <exception cref="IndexOutOfRangeException"/>
        public static FsmTransition GetTransition(this FsmState state, int transitionIndex)
        {
            state.ThrowIfNull(nameof(state));

            return state.Transitions[transitionIndex];
        }

        /// <summary>
        /// Gets the first transition in the <see cref="FsmState"/> that optionally matches the specified <paramref name="predicate"/>.
        /// </summary>
        /// <param name="state">The state to look through.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <returns>The <see cref="FsmTransition"/> that was matched if one was found; otherwise, <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static FsmTransition? GetTransition(this FsmState state, Func<FsmTransition, bool>? predicate = null)
        {
            return state.GetTransitions(predicate).FirstOrDefault();
        }

        /// <summary>
        /// Gets the transitions in the <see cref="FsmState"/> that optionally match the specified <paramref name="predicate"/>.
        /// </summary>
        /// <returns>A collection of <see cref="FsmTransition"/> objects which matched the specified predicate.</returns>
        ///
        /// <inheritdoc cref="GetTransition(FsmState, Func{FsmTransition, bool})"/>
        public static IEnumerable<FsmTransition> GetTransitions(this FsmState state, Func<FsmTransition, bool>? predicate = null)
        {
            state.ThrowIfNull(nameof(state));

            return (predicate is null) ? state.Transitions.AsEnumerable() : state.Transitions.Where(predicate);
        }

        #endregion

        #region Replace Methods

        /// <summary>
        /// Replaces the transition with the specified <paramref name="eventName"/> in the <see cref="FsmState"/> with the given <paramref name="replacementTransition"/>.
        /// </summary>
        /// <param name="eventName">The transition's event name to look for.</param>
        ///
        /// <inheritdoc cref="ReplaceTransition(FsmState, Func{FsmTransition, bool}, FsmTransition)"/>
        public static bool ReplaceTransition(this FsmState state, string eventName, FsmTransition replacementTransition)
        {
            eventName.ThrowIfNull(nameof(eventName));

            return state.ReplaceTransition((transition) => transition.EventName == eventName, replacementTransition);
        }

        /// <summary>
        /// Replaces the transition at the specified <paramref name="transitionIndex"/> in the <see cref="FsmState"/> with the given <paramref name="replacementTransition"/>.
        /// </summary>
        /// <param name="transitionIndex">The index of the transition in the <see cref="FsmState"/> to replace.</param>
        ///
        /// <inheritdoc cref="ReplaceTransition(FsmState, Func{FsmTransition, bool}, FsmTransition)"/>
        /// <exception cref="IndexOutOfRangeException"/>
        public static void ReplaceTransition(this FsmState state, int transitionIndex, FsmTransition replacementTransition)
        {
            state.ThrowIfNull(nameof(state));
            replacementTransition.ThrowIfNull(nameof(replacementTransition));

            state.Transitions[transitionIndex] = replacementTransition;
        }

        /// <summary>
        /// Replaces the specified <paramref name="targetTransition"/> in the <see cref="FsmState"/> with the given <paramref name="replacementTransition"/>.
        /// </summary>
        /// <param name="targetTransition">The transition in <paramref name="state"/> to replace.</param>
        ///
        /// <inheritdoc cref="ReplaceTransition(FsmState, Func{FsmTransition, bool}, FsmTransition)"/>
        public static bool ReplaceTransition(this FsmState state, FsmTransition targetTransition, FsmTransition replacementTransition)
        {
            targetTransition.ThrowIfNull(nameof(targetTransition));

            return state.ReplaceTransition((transition) => transition == targetTransition, replacementTransition);
        }

        /// <summary>
        /// Replaces the first transition in the <see cref="FsmState"/> that matches the specified predicate with the given <paramref name="replacementTransition"/>.
        /// </summary>
        /// <param name="state">The state to look through.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <param name="replacementTransition">The transition to replace the found transition with.</param>
        /// <returns><see langword="true"/> if an item was successfully found and replaced; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static bool ReplaceTransition(this FsmState state, Func<FsmTransition, bool> predicate, FsmTransition replacementTransition)
        {
            state.ThrowIfNull(nameof(state));
            predicate.ThrowIfNull(nameof(predicate));
            replacementTransition.ThrowIfNull(nameof(replacementTransition));

            Predicate<FsmTransition> actualPredicate = new(predicate);
            int targetIndex = Array.FindIndex(state.Transitions, actualPredicate);
            if (targetIndex is -1)
                return false;

            state.Transitions[targetIndex] = replacementTransition;
            return true;
        }

        #endregion

        #region Add Methods

        /// <summary>
        /// Creates a new transition and adds it to the <see cref="FsmState"/>.
        /// </summary>
        /// <param name="triggerEvent">The event that triggers the transition.</param>
        /// <param name="toStateName">The state to transition to.</param>
        /// <returns>The transition that was created.</returns>
        ///
        /// <inheritdoc cref="AddTransition(FsmState, FsmTransition)"/>
        public static FsmTransition AddTransition(this FsmState state, FsmEvent triggerEvent, string toStateName)
        {
            state.ThrowIfNull(nameof(state));
            triggerEvent.ThrowIfNull(nameof(triggerEvent));
            toStateName.ThrowIfNull(nameof(toStateName));

            FsmTransition transition = new FsmTransition
            {
                FsmEvent = triggerEvent,
                ToState = toStateName
            };

            state.AddTransition(transition);
            return transition;
        }

        /// <summary>
        /// Adds the specified transition to the <see cref="FsmState"/>.
        /// </summary>
        /// <param name="state">The state to add the transition to.</param>
        /// <param name="transition">The transition to add to the state.</param>
        /// <exception cref="ArgumentNullException"/>
        public static void AddTransition(this FsmState state, FsmTransition transition)
        {
            state.ThrowIfNull(nameof(state));
            transition.ThrowIfNull(nameof(transition));

            state.Transitions = state.Transitions.Add(transition);
        }

        /// <summary>
        /// Adds the specified transitions to the <see cref="FsmState"/>.
        /// </summary>
        /// <param name="transitions">The transitions to add to the state.</param>
        ///
        /// <inheritdoc cref="AddTransition(FsmState, FsmTransition)"/>
        public static void AddTransitions(this FsmState state, params FsmTransition[] transitions)
        {
            state.ThrowIfNull(nameof(state));
            transitions.ThrowIfNull(nameof(transitions));

            state.Transitions = state.Transitions.AddRange(transitions);
        }

        #endregion

        #region Insert Methods

        /// <summary>
        /// Inserts the specified transition into the <see cref="FsmState"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="state">The state to insert the transition into.</param>
        /// <param name="index">The index in the <see cref="FsmState"/> to insert at.</param>
        /// <param name="transition">The transition to insert into the state.</param>
        /// <exception cref="ArgumentNullException"/>
        public static void InsertTransition(this FsmState state, int index, FsmTransition transition)
        {
            state.ThrowIfNull(nameof(state));
            transition.ThrowIfNull(nameof(transition));

            state.Transitions = state.Transitions.Insert(index, transition);
        }

        /// <summary>
        /// Inserts the specified transitions into the <see cref="FsmState"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="state">The state to insert the transitions into.</param>
        /// <param name="index">The index in the <see cref="FsmState"/> to insert at.</param>
        /// <param name="transitions">The transitions to insert into the state.</param>
        /// <exception cref="ArgumentNullException"/>
        public static void InsertTransitions(this FsmState state, int index, params FsmTransition[] transitions)
        {
            state.ThrowIfNull(nameof(state));
            transitions.ThrowIfNull(nameof(transitions));

            state.Transitions = state.Transitions.InsertRange(index, transitions);
        }

        #endregion

        #region Remove Methods

        /// <summary>
        /// Removes the first transition with the specified <paramref name="eventName"/> from the <see cref="FsmState"/>.
        /// </summary>
        /// <param name="eventName">The event name of the transition to remove.</param>
        ///
        /// <inheritdoc cref="RemoveTransition(FsmState, Func{FsmTransition, bool})"/>
        public static bool RemoveTransition(this FsmState state, string eventName)
        {
            return state.RemoveTransition((transition) => transition.EventName == eventName);
        }

        /// <summary>
        /// Removes the transition at the specified <paramref name="transitionIndex"/> from the <see cref="FsmState"/>.
        /// </summary>
        /// <param name="transitionIndex">The index of the transition to remove.</param>
        /// <returns/>
        ///
        /// <inheritdoc cref="RemoveTransition(FsmState, Func{FsmTransition, bool})"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static void RemoveTransition(this FsmState state, int transitionIndex)
        {
            state.ThrowIfNull(nameof(state));

            state.Transitions = state.Transitions.RemoveAt(transitionIndex);
        }

        /// <summary>
        /// Removes the specified transition from the <see cref="FsmState"/>.
        /// </summary>
        /// <param name="targetTransition">The transition to remove.</param>
        ///
        /// <inheritdoc cref="RemoveTransition(FsmState, Func{FsmTransition, bool})"/>
        public static bool RemoveTransition(this FsmState state, FsmTransition targetTransition)
        {
            return state.RemoveTransition((transition) => transition == targetTransition);
        }

        /// <summary>
        /// Removes the first transition that matches the specified <paramref name="predicate"/> from the <see cref="FsmState"/>.
        /// </summary>
        /// <param name="state">The state to remove from.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <returns><see langword="true"/> if an item was successfully found and removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static bool RemoveTransition(this FsmState state, Func<FsmTransition, bool> predicate)
        {
            state.ThrowIfNull(nameof(state));
            predicate.ThrowIfNull(nameof(predicate));

            var withoutTransition = state.Transitions.WithoutFirst(predicate).ToArray();
            bool wasRemoved = withoutTransition.Length != state.Transitions.Length;

            state.Transitions = withoutTransition;
            return wasRemoved;
        }

        /// <summary>
        /// Removes the specified transitions from the <see cref="FsmState"/>.
        /// </summary>
        /// <param name="targetTransitions">The transitions to remove.</param>
        ///
        /// <inheritdoc cref="RemoveTransition(FsmState, Func{FsmTransition, bool})"/>
        public static bool RemoveTransitions(this FsmState state, params FsmTransition[] targetTransitions)
        {
            return state.RemoveTransitions((transition) => targetTransitions.Contains(transition));
        }

        /// <summary>
        /// Removes the transitions that match the specified <paramref name="predicate"/> from the <see cref="FsmState"/>.
        /// </summary>
        ///
        /// <inheritdoc cref="RemoveTransition(FsmState, Func{FsmTransition, bool})"/>
        public static bool RemoveTransitions(this FsmState state, Func<FsmTransition, bool> predicate)
        {
            state.ThrowIfNull(nameof(state));
            predicate.ThrowIfNull(nameof(predicate));

            var withoutTransitions = state.Transitions.Where((transition) => !predicate(transition)).ToArray();
            bool anyRemoved = withoutTransitions.Length != state.Transitions.Length;

            state.Transitions = withoutTransitions;
            return anyRemoved;
        }

        #endregion
    }
}