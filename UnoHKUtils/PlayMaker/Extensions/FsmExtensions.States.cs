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
        // Extensions for Fsm with FsmState

        #region Get Methods

        /// <summary>
        /// Gets the state in the <see cref="Fsm"/> that has the specified <paramref name="stateName"/>.
        /// </summary>
        /// <param name="stateName">The name of the state to get.</param>
        ///
        /// <inheritdoc cref="GetState(Fsm, Func{FsmState, bool}?)"/>
        public static FsmState? GetState(this Fsm fsm, string stateName)
        {
            stateName.ThrowIfNull(nameof(stateName));

            return fsm.GetState((state) => state.Name == stateName);
        }

        /// <summary>
        /// Gets the state in the <see cref="Fsm"/> at the specified <paramref name="stateIndex"/>.
        /// </summary>
        /// <param name="stateIndex">The index of the state to get.</param>
        /// <returns>The state retrieved from the specified index.</returns>
        ///
        /// <inheritdoc cref="GetState(Fsm, Func{FsmState, bool}?)"/>
        /// <exception cref="IndexOutOfRangeException"/>
        public static FsmState GetState(this Fsm fsm, int stateIndex)
        {
            fsm.ThrowIfNull(nameof(fsm));

            return fsm.States[stateIndex];
        }

        /// <summary>
        /// Gets the first state in the <see cref="Fsm"/> that optionally matches a specified <paramref name="predicate"/>.
        /// </summary>
        /// <returns>The <see cref="FsmState"/> that was matched if one was found; otherwise, <see langword="null"/>.</returns>
        ///
        /// <inheritdoc cref="GetStates(Fsm, Func{FsmState, bool}?)"/>
        public static FsmState? GetState(this Fsm fsm, Func<FsmState, bool>? predicate = null)
        {
            return fsm.GetStates(predicate).FirstOrDefault();
        }

        /// <summary>
        /// Gets the states in the <see cref="Fsm"/> that optionally match a specified <paramref name="predicate"/>.
        /// </summary>
        /// <param name="fsm">The FSM to use.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <returns>A collection of <see cref="FsmState"/> objects.</returns>
        public static IEnumerable<FsmState> GetStates(this Fsm fsm, Func<FsmState, bool>? predicate = null)
        {
            fsm.ThrowIfNull(nameof(fsm));

            return (predicate is null) ? fsm.States.AsEnumerable() : fsm.States.Where(predicate);
        }

        #endregion

        #region Replace Methods

        /// <summary>
        /// Replaces the state in the <see cref="Fsm"/> that has the specified <paramref name="stateName"/> with the given <paramref name="replacementState"/>.
        /// </summary>
        /// <param name="stateName">The name of the state to replace.</param>
        ///
        /// <inheritdoc cref="ReplaceState(Fsm, Func{FsmState, bool}, FsmState)"/>
        public static bool ReplaceState(this Fsm fsm, string stateName, FsmState replacementState)
        {
            stateName.ThrowIfNull(nameof(stateName));

            return fsm.ReplaceState((state) => state.Name == stateName, replacementState);
        }

        /// <summary>
        /// Replaces the state at the specified <paramref name="stateIndex"/> in the <see cref="Fsm"/> with the given <paramref name="replacementState"/>.
        /// </summary>
        /// <param name="stateIndex">The index of the state in the <see cref="Fsm"/> to replace.</param>
        /// <returns/>
        ///
        /// <inheritdoc cref="ReplaceState(Fsm, Func{FsmState, bool}, FsmState)"/>
        /// <exception cref="IndexOutOfRangeException"/>
        public static void ReplaceState(this Fsm fsm, int stateIndex, FsmState replacementState)
        {
            fsm.ThrowIfNull(nameof(fsm));
            replacementState.ThrowIfNull(nameof(replacementState));

            fsm.States[stateIndex] = replacementState;
        }

        /// <summary>
        /// Replaces the first state in the <see cref="Fsm"/> that is equal to <paramref name="targetState"/> with the given <paramref name="replacementState"/>.
        /// </summary>
        /// <param name="targetState">The state in the <see cref="Fsm"/> to replace.</param>
        ///
        /// <inheritdoc cref="ReplaceState(Fsm, Func{FsmState, bool}, FsmState)"/>
        public static bool ReplaceState(this Fsm fsm, FsmState targetState, FsmState replacementState)
        {
            targetState.ThrowIfNull(nameof(targetState));

            return fsm.ReplaceState((state) => state == targetState, replacementState);
        }

        /// <summary>
        /// Replaces the first state in the <see cref="Fsm"/> that matches the specified predicate with
        /// the given <paramref name="replacementState"/>.
        /// </summary>
        /// <param name="fsm">The FSM to look through.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <param name="replacementState">The state to replace the found state with.</param>
        /// <returns><see langword="true"/> if an item was successfully found and replaced; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static bool ReplaceState(this Fsm fsm, Func<FsmState, bool> predicate, FsmState replacementState)
        {
            fsm.ThrowIfNull(nameof(fsm));
            predicate.ThrowIfNull(nameof(predicate));
            replacementState.ThrowIfNull(nameof(replacementState));

            int targetIndex = Array.FindIndex(fsm.States, (state) => predicate(state));
            if (targetIndex is -1)
                return false;

            fsm.States[targetIndex] = replacementState;
            return true;
        }

        #endregion

        #region Add Methods

        /// <summary>
        /// Adds the specified state to the <see cref="Fsm"/>.
        /// </summary>
        /// <param name="fsm">The FSM to add to.</param>
        /// <param name="state">The state to add.</param>
        public static void AddState(this Fsm fsm, FsmState state)
        {
            fsm.ThrowIfNull(nameof(fsm));
            state.ThrowIfNull(nameof(state));

            fsm.States = fsm.States.Add(state);
        }

        /// <summary>
        /// Adds the specified states to the <see cref="Fsm"/>.
        /// </summary>
        /// <param name="fsm">The FSM to add to.</param>
        /// <param name="states">The states to add.</param>
        public static void AddStates(this Fsm fsm, params FsmState[] states)
        {
            fsm.ThrowIfNull(nameof(fsm));
            states.ThrowIfNull(nameof(states));

            fsm.States = fsm.States.AddRange(states);
        }

        #endregion

        #region Insert Methods

        /// <summary>
        /// Inserts the specified state into the <see cref="Fsm"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="state">The state to insert.</param>
        ///
        /// <inheritdoc cref="InsertStates(Fsm, int, FsmState[])"/>
        public static void InsertState(this Fsm fsm, int index, FsmState state)
        {
            fsm.ThrowIfNull(nameof(fsm));
            state.ThrowIfNull(nameof(state));

            fsm.States = fsm.States.Insert(index, state);
        }

        /// <summary>
        /// Inserts the specified states into the <see cref="Fsm"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="fsm">The FSM to insert into.</param>
        /// <param name="index">The index in the <see cref="Fsm"/> to insert at.</param>
        /// <param name="states">The states to insert.</param>
        public static void InsertStates(this Fsm fsm, int index, params FsmState[] states)
        {
            fsm.ThrowIfNull(nameof(fsm));
            states.ThrowIfNull(nameof(states));

            fsm.States = fsm.States.InsertRange(index, states);
        }

        #endregion

        #region Remove Methods

        /// <summary>
        /// Removes the first state with the specified <paramref name="stateName"/> from the <see cref="Fsm"/>.
        /// </summary>
        /// <param name="stateName">The name of the state to remove.</param>
        ///
        /// <inheritdoc cref="RemoveStates(Fsm, Func{FsmState, bool})"/>
        public static bool RemoveState(this Fsm fsm, string stateName)
        {
            return fsm.RemoveState((state) => state.Name == stateName);
        }

        /// <summary>
        /// Removes the state at the specified <paramref name="stateIndex"/> from the <see cref="Fsm"/>.
        /// </summary>
        /// <param name="stateIndex">The index of the state to remove.</param>
        /// <returns/>
        ///
        /// <inheritdoc cref="RemoveStates(Fsm, Func{FsmState, bool})"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static void RemoveState(this Fsm fsm, int stateIndex)
        {
            fsm.ThrowIfNull(nameof(fsm));

            fsm.States = fsm.States.RemoveAt(stateIndex);
        }

        /// <summary>
        /// Removes the specified state from the <see cref="Fsm"/>.
        /// </summary>
        /// <param name="targetState">The state to remove.</param>
        ///
        /// <inheritdoc cref="RemoveStates(Fsm, Func{FsmState, bool})"/>
        public static bool RemoveState(this Fsm fsm, FsmState targetState)
        {
            targetState.ThrowIfNull(nameof(targetState));

            return fsm.RemoveState((state) => state == targetState);
        }

        /// <summary>
        /// Removes the first state that matches the specified <paramref name="predicate"/> from the <see cref="Fsm"/>.
        /// </summary>
        ///
        /// <inheritdoc cref="RemoveStates(Fsm, Func{FsmState, bool})"/>
        public static bool RemoveState(this Fsm fsm, Func<FsmState, bool> predicate)
        {
            fsm.ThrowIfNull(nameof(fsm));
            predicate.ThrowIfNull(nameof(predicate));

            var withoutState = fsm.States.WithoutFirst(predicate).ToArray();
            bool wasRemoved = withoutState.Length != fsm.States.Length;

            fsm.States = withoutState;
            return wasRemoved;
        }

        /// <summary>
        /// Removes the specified states from the <see cref="Fsm"/>.
        /// </summary>
        /// <param name="targetStates">The states to remove.</param>
        ///
        /// <inheritdoc cref="RemoveStates(Fsm, Func{FsmState, bool})"/>
        public static bool RemoveStates(this Fsm fsm, params FsmState[] targetStates)
        {
            targetStates.ThrowIfNull(nameof(targetStates));

            return fsm.RemoveStates((state) => targetStates.Contains(state));
        }

        /// <summary>
        /// Removes the states from the <see cref="Fsm"/> that match the specified <paramref name="predicate"/>.
        /// </summary>
        /// <param name="fsm">The FSM to remove from.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <returns><see langword="true"/> if an item was successfully found and removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static bool RemoveStates(this Fsm fsm, Func<FsmState, bool> predicate)
        {
            fsm.ThrowIfNull(nameof(fsm));
            predicate.ThrowIfNull(nameof(predicate));

            var withoutStates = fsm.States.Where((state) => !predicate(state)).ToArray();
            bool anyRemoved = withoutStates.Length != fsm.States.Length;

            fsm.States = withoutStates;
            return anyRemoved;
        }

        #endregion
    }
}