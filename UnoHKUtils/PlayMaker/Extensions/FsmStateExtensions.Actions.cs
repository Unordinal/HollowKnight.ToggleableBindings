#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using UnoHKUtils.Extensions;

namespace UnoHKUtils.PlayMaker.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="PlayMaker"/> <see cref="FsmState"/> objects.
    /// Some methods - mostly ones that take an index - exist only for API completeness.
    /// </summary>
    public static partial class FsmStateExtensions
    {
        // Extensions for FsmState with FsmStateAction

        #region Get Methods

        /// <summary>
        /// Gets the action in the <see cref="FsmState"/> that has the specified <paramref name="actionName"/>.
        /// </summary>
        /// <param name="actionName">The name of the action to get.</param>
        ///
        /// <inheritdoc cref="GetAction(FsmState, Func{FsmStateAction, bool})"/>
        public static FsmStateAction? GetAction(this FsmState state, string actionName)
        {
            actionName.ThrowIfNull(nameof(actionName));

            return state.GetAction((action) => action.Name == actionName);
        }

        /// <summary>
        /// Gets the action in the <see cref="FsmState"/> that has the specified <paramref name="actionName"/>.
        /// </summary>
        /// <param name="actionName">The name of the action to get.</param>
        ///
        /// <inheritdoc cref="GetAction{T}(FsmState, Func{T, bool}?)"/>
        public static T? GetAction<T>(this FsmState state, string actionName) where T : FsmStateAction
        {
            actionName.ThrowIfNull(nameof(actionName));

            return state.GetAction<T>((action) => action.Name == actionName);
        }

        /// <summary>
        /// Gets the action in the <see cref="FsmState"/> at the specified <paramref name="actionIndex"/>.
        /// </summary>
        /// <param name="actionIndex">The index of the action to get.</param>
        /// <returns>The <see cref="FsmStateAction"/> at the specified index.</returns>
        ///
        /// <inheritdoc cref="GetAction(FsmState, Func{FsmStateAction, bool})"/>
        public static FsmStateAction GetAction(this FsmState state, int actionIndex)
        {
            state.ThrowIfNull(nameof(state));

            return state.Actions[actionIndex];
        }

        /// <summary>
        /// Gets the action in the <see cref="FsmState"/> at the specified <paramref name="actionIndex"/>.
        /// </summary>
        /// <param name="actionIndex">The index of the action to get.</param>
        /// <returns>The <typeparamref name="T"/> at the specified index.</returns>
        ///
        /// <inheritdoc cref="GetAction{T}(FsmState, Func{T, bool}?)"/>
        /// <exception cref="IndexOutOfRangeException"/>
        public static T GetAction<T>(this FsmState state, int actionIndex) where T : FsmStateAction
        {
            return (T)state.GetAction(actionIndex);
        }

        /// <summary>
        /// Gets the first action in the <see cref="FsmState"/> that matches the specified <paramref name="predicate"/>.
        /// </summary>
        /// <returns>The <see cref="FsmStateAction"/> that was matched if one was found; otherwise, <see langword="null"/>.</returns>
        ///
        /// <inheritdoc cref="GetAction{T}(FsmState, Func{T, bool}?)"/>
        public static FsmStateAction? GetAction(this FsmState state, Func<FsmStateAction, bool>? predicate = null)
        {
            return state.GetActions(predicate).FirstOrDefault();
        }

        /// <summary>
        /// Gets the first action in the <see cref="FsmState"/> that is of the specified type and optionally matches the specified <paramref name="predicate"/>.
        /// </summary>
        /// <typeparam name="T">The type of state action.</typeparam>
        /// <param name="state">The state to look through.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <returns>The <typeparamref name="T"/> that was matched if one was found; otherwise, <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static T? GetAction<T>(this FsmState state, Func<T, bool>? predicate = null) where T : FsmStateAction
        {
            return state.GetActions(predicate).FirstOrDefault();
        }

        /// <summary>
        /// Gets the actions in the <see cref="FsmState"/> that optionally match a specified <paramref name="predicate"/>.
        /// </summary>
        /// <returns>A collection of <see cref="FsmStateAction"/> objects.</returns>
        ///
        /// <inheritdoc cref="GetAction{T}(FsmState, Func{T, bool})"/>
        public static IEnumerable<FsmStateAction> GetActions(this FsmState state, Func<FsmStateAction, bool>? predicate = null)
        {
            state.ThrowIfNull(nameof(state));

            return (predicate is null) ? state.Actions.AsEnumerable() : state.Actions.Where(predicate);
        }

        /// <summary>
        /// Gets the actions in the <see cref="FsmState"/> that are of the specified type and optionally match a specified <paramref name="predicate"/>.
        /// </summary>
        /// <returns>A collection of <typeparamref name="T"/> objects.</returns>
        ///
        /// <inheritdoc cref="GetAction{T}(FsmState, Func{T, bool})"/>
        public static IEnumerable<T> GetActions<T>(this FsmState state, Func<T, bool>? predicate = null) where T : FsmStateAction
        {
            state.ThrowIfNull(nameof(state));

            var output = state.Actions.OfType<T>();
            if (predicate is not null)
                output = output.Where((action) => predicate(action));

            return output;
        }

        #endregion

        #region Replace Methods

        /// <summary>
        /// Replaces the action with the specified <paramref name="actionName"/> in the <see cref="FsmState"/> with the given <paramref name="replacementAction"/>.
        /// </summary>
        /// <param name="actionName">The name of the action to replace.</param>
        ///
        /// <inheritdoc cref="ReplaceAction(FsmState, Func{FsmStateAction, bool}, FsmStateAction)"/>
        public static bool ReplaceAction(this FsmState state, string actionName, FsmStateAction replacementAction)
        {
            return state.ReplaceAction((action) => action.Name == actionName, replacementAction);
        }

        /// <summary>
        /// Replaces the action at the specified <paramref name="actionIndex"/> in the <see cref="FsmState"/> with the given <paramref name="replacementAction"/>.
        /// </summary>
        /// <param name="actionIndex">The index of the action in the <see cref="FsmState"/> to replace.</param>
        /// <returns/>
        ///
        /// <inheritdoc cref="ReplaceAction(FsmState, Func{FsmStateAction, bool}, FsmStateAction)"/>
        /// <exception cref="IndexOutOfRangeException"/>
        public static void ReplaceAction(this FsmState state, int actionIndex, FsmStateAction replacementAction)
        {
            state.ThrowIfNull(nameof(state));
            replacementAction.ThrowIfNull(nameof(replacementAction));

            state.Actions[actionIndex] = replacementAction;
        }

        /// <summary>
        /// Replaces the first action in the <see cref="FsmState"/> that is equal to <paramref name="targetAction"/> with the given <paramref name="replacementAction"/>.
        /// </summary>
        /// <param name="targetAction">The action in the <see cref="FsmState"/> to replace.</param>
        ///
        /// <inheritdoc cref="ReplaceAction(FsmState, Func{FsmStateAction, bool}, FsmStateAction)"/>
        public static bool ReplaceAction(this FsmState state, FsmStateAction targetAction, FsmStateAction replacementAction)
        {
            return state.ReplaceAction((action) => action == targetAction, replacementAction);
        }

        /// <summary>
        /// Replaces the first action in the <see cref="FsmState"/> that matches the specified predicate with
        /// the given <paramref name="replacementAction"/>.
        /// </summary>
        /// <param name="state">The state to look through.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <param name="replacementAction">The action to replace the found action with.</param>
        /// <returns><see langword="true"/> if an item was successfully found and replaced; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static bool ReplaceAction(this FsmState state, Func<FsmStateAction, bool> predicate, FsmStateAction replacementAction)
        {
            state.ThrowIfNull(nameof(state));
            predicate.ThrowIfNull(nameof(predicate));
            replacementAction.ThrowIfNull(nameof(replacementAction));

            int targetIndex = Array.FindIndex(state.Actions, (action) => predicate(action));
            if (targetIndex is -1)
                return false;

            state.Actions[targetIndex] = replacementAction;
            return true;
        }

        /// <summary>
        /// Replaces the first action in the <see cref="FsmState"/> that is the specified type and matches the
        /// specified <paramref name="predicate"/> with the given <paramref name="replacementAction"/>.
        /// </summary>
        /// <typeparam name="T">The type of action to replace.</typeparam>
        ///
        /// <inheritdoc cref="ReplaceAction(FsmState, Func{FsmStateAction, bool}, FsmStateAction)"/>
        public static bool ReplaceAction<T>(this FsmState state, Func<T, bool> predicate, FsmStateAction replacementAction) where T : FsmStateAction
        {
            state.ThrowIfNull(nameof(state));
            predicate.ThrowIfNull(nameof(predicate));
            replacementAction.ThrowIfNull(nameof(replacementAction));

            int targetIndex = Array.FindIndex(state.Actions, (action) => (action is T actionT) && predicate(actionT));
            if (targetIndex is -1)
                return false;

            state.Actions[targetIndex] = replacementAction;
            return true;
        }

        /// <summary>
        /// Replaces the first action in the <see cref="FsmState"/> that is the specified type with the
        /// given <paramref name="replacementAction"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        ///
        /// <inheritdoc cref="ReplaceAction{T}(FsmState, Func{T, bool}, FsmStateAction)"/>
        public static bool ReplaceAction<T>(this FsmState state, FsmStateAction replacementAction) where T : FsmStateAction
        {
            return state.ReplaceAction<T>((action) => true, replacementAction);
        }

        #endregion

        #region Add Methods

        /// <summary>
        /// Adds the specified action to the <see cref="FsmState"/>.
        /// </summary>
        /// <param name="state">The state to add to.</param>
        /// <param name="action">The action to add.</param>
        /// <exception cref="ArgumentNullException"/>
        public static void AddAction(this FsmState state, FsmStateAction action)
        {
            state.ThrowIfNull(nameof(state));
            action.ThrowIfNull(nameof(action));

            state.Actions = state.Actions.Add(action);
        }

        /// <summary>
        /// Adds the specified actions to the <see cref="FsmState"/>.
        /// </summary>
        /// <param name="actions">The actions to add.</param>
        ///
        /// <inheritdoc cref="AddAction(FsmState, FsmStateAction)"/>
        public static void AddActions(this FsmState state, params FsmStateAction[] actions)
        {
            state.ThrowIfNull(nameof(state));
            actions.ThrowIfNull(nameof(actions));

            state.Actions = state.Actions.AddRange(actions);
        }

        #endregion

        #region Insert Methods

        /// <summary>
        /// Inserts the specified action into the <see cref="FsmState"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="state">The state to insert into.</param>
        /// <param name="index">The index in the <see cref="FsmState"/> to insert at.</param>
        /// <param name="action">The action to insert.</param>
        /// <exception cref="ArgumentNullException"/>
        public static void InsertAction(this FsmState state, int index, FsmStateAction action)
        {
            state.ThrowIfNull(nameof(state));
            action.ThrowIfNull(nameof(action));

            state.Actions = state.Actions.Insert(index, action);
        }

        /// <summary>
        /// Inserts the specified actions into the <see cref="FsmState"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="actions">The actions to insert.</param>
        ///
        /// <inheritdoc cref="InsertAction(FsmState, int, FsmStateAction)"/>
        public static void InsertActions(this FsmState state, int index, params FsmStateAction[] actions)
        {
            state.ThrowIfNull(nameof(state));
            actions.ThrowIfNull(nameof(actions));

            state.Actions = state.Actions.InsertRange(index, actions);
        }

        #endregion

        #region Remove Methods

        /// <summary>
        /// Removes the first action with the specified <paramref name="actionName"/> from the <see cref="FsmState"/>.
        /// </summary>
        /// <param name="actionName">The name of the action to remove.</param>
        ///
        /// <inheritdoc cref="RemoveActions{T}(FsmState, Func{T, bool}?)"/>
        public static bool RemoveAction(this FsmState state, string actionName)
        {
            actionName.ThrowIfNull(nameof(actionName));

            return state.RemoveAction((action) => action.Name == actionName);
        }

        /// <summary>
        /// Removes the action at the specified <paramref name="actionIndex"/> from the <see cref="FsmState"/>.
        /// </summary>
        /// <param name="actionIndex">The index of the action to remove.</param>
        /// <returns/>
        ///
        /// <inheritdoc cref="RemoveActions{T}(FsmState, Func{T, bool}?)"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static void RemoveAction(this FsmState state, int actionIndex)
        {
            state.ThrowIfNull(nameof(state));

            state.Actions = state.Actions.RemoveAt(actionIndex);
        }

        /// <summary>
        /// Removes the specified action from the <see cref="FsmState"/>.
        /// </summary>
        /// <param name="targetAction">The action to remove.</param>
        ///
        /// <inheritdoc cref="RemoveActions{T}(FsmState, Func{T, bool}?)"/>
        public static bool RemoveAction(this FsmState state, FsmStateAction targetAction)
        {
            targetAction.ThrowIfNull(nameof(targetAction));

            return state.RemoveAction((action) => action == targetAction);
        }

        /// <summary>
        /// Removes the first action that matches the specified <paramref name="predicate"/> from the <see cref="FsmState"/>.
        /// </summary>
        ///
        /// <inheritdoc cref="RemoveActions{T}(FsmState, Func{T, bool}?)"/>
        public static bool RemoveAction(this FsmState state, Func<FsmStateAction, bool> predicate)
        {
            state.ThrowIfNull(nameof(state));
            predicate.ThrowIfNull(nameof(predicate));

            var withoutAction = state.Actions.WithoutFirst(predicate).ToArray();
            bool wasRemoved = withoutAction.Length != state.Actions.Length;

            state.Actions = withoutAction;
            return wasRemoved;
        }

        /// <summary>
        /// Removes the first action from the <see cref="FsmState"/> that is the specified type and optionally
        /// matches a specified <paramref name="predicate"/>.
        /// </summary>
        ///
        /// <inheritdoc cref="RemoveActions{T}(FsmState, Func{T, bool}?)"/>
        public static bool RemoveAction<T>(this FsmState state, Func<T, bool>? predicate = null) where T : FsmStateAction
        {
            state.ThrowIfNull(nameof(state));

            var filtered = state.Actions.OfType<T>();
            if (predicate is not null)
                filtered = filtered.Where(predicate);

            var toRemove = filtered.FirstOrDefault();
            if (toRemove is null)
                return false;

            var withoutAction = state.Actions.WithoutFirst((action) => action == toRemove).ToArray();
            bool wasRemoved = withoutAction.Length != state.Actions.Length;

            state.Actions = withoutAction;
            return wasRemoved;
        }

        /// <summary>
        /// Removes the specified actions from the <see cref="FsmState"/>.
        /// </summary>
        /// <param name="targetActions">The actions to remove.</param>
        ///
        /// <inheritdoc cref="RemoveActions{T}(FsmState, Func{T, bool}?)"/>
        public static bool RemoveActions(this FsmState state, params FsmStateAction[] targetActions)
        {
            targetActions.ThrowIfNull(nameof(targetActions));

            return state.RemoveActions((action) => targetActions.Contains(action));
        }

        /// <summary>
        /// Removes the actions from the <see cref="FsmState"/> that match the specified <paramref name="predicate"/>.
        /// </summary>
        ///
        /// <inheritdoc cref="RemoveActions{T}(FsmState, Func{T, bool}?)"/>
        public static bool RemoveActions(this FsmState state, Func<FsmStateAction, bool> predicate)
        {
            state.ThrowIfNull(nameof(state));
            predicate.ThrowIfNull(nameof(predicate));

            var withoutActions = state.Actions.Where((action) => !predicate(action)).ToArray();
            bool anyRemoved = withoutActions.Length != state.Actions.Length;

            state.Actions = withoutActions;
            return anyRemoved;
        }

        /// <summary>
        /// Removes the actions from the <see cref="FsmState"/> that are of the specified type and optionally match a
        /// specified <paramref name="predicate"/>.
        /// </summary>
        /// <typeparam name="T">The type of action to remove.</typeparam>
        /// <param name="state">The state to remove from.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <returns><see langword="true"/> if an item was successfully found and removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static bool RemoveActions<T>(this FsmState state, Func<T, bool>? predicate = null) where T : FsmStateAction
        {
            state.ThrowIfNull(nameof(state));

            var filtered = state.Actions.OfType<T>();
            if (predicate is not null)
                filtered = filtered.Where(predicate);

            var withoutActions = state.Actions.Except(filtered.Cast<FsmStateAction>()).ToArray();
            bool anyRemoved = withoutActions.Length != state.Actions.Length;

            state.Actions = withoutActions;
            return anyRemoved;
        }

        #endregion
    }
}