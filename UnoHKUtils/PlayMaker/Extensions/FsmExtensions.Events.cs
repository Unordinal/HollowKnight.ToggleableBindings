#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using UnoHKUtils.Extensions;

namespace UnoHKUtils.PlayMaker.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="PlayMaker"/> <see cref="Fsm"/> objects.
    /// Some methods - mostly ones that take an index - pretty much exist only for API completeness.
    /// </summary>
    public static partial class FsmExtensions
    {
        // Extensions for Fsm with FsmEvent

        #region Get Methods

        /// <summary>
        /// Gets the event in the <see cref="Fsm"/> that has the specified <paramref name="eventName"/>.
        /// </summary>
        /// <param name="eventName">The name of the event to get.</param>
        ///
        /// <inheritdoc cref="GetEvent(Fsm, Func{FsmEvent, bool}?)"/>
        public static FsmEvent? GetEvent(this Fsm fsm, string eventName)
        {
            eventName.ThrowIfNull(nameof(eventName));

            return fsm.GetEvent((evnt) => evnt.Name == eventName);
        }

        /// <summary>
        /// Gets the event in the <see cref="Fsm"/> at the specified <paramref name="eventIndex"/>.
        /// </summary>
        /// <param name="eventIndex">The index of the event to get.</param>
        /// <returns>The event retrieved from the specified index.</returns>
        ///
        /// <inheritdoc cref="GetEvent(Fsm, Func{FsmEvent, bool}?)"/>
        /// <exception cref="IndexOutOfRangeException"/>
        public static FsmEvent GetEvent(this Fsm fsm, int eventIndex)
        {
            fsm.ThrowIfNull(nameof(fsm));

            return fsm.Events[eventIndex];
        }

        /// <summary>
        /// Gets the first event in the <see cref="Fsm"/> that optionally matches a specified <paramref name="predicate"/>.
        /// </summary>
        /// <returns>The <see cref="FsmEvent"/> that was matched if one was found; otherwise, <see langword="null"/>.</returns>
        ///
        /// <inheritdoc cref="GetEvents(Fsm, Func{FsmEvent, bool}?)"/>
        public static FsmEvent? GetEvent(this Fsm fsm, Func<FsmEvent, bool>? predicate = null)
        {
            return fsm.GetEvents(predicate).FirstOrDefault();
        }

        /// <summary>
        /// Gets the events in the <see cref="Fsm"/> that optionally match a specified <paramref name="predicate"/>.
        /// </summary>
        /// <param name="fsm">The FSM to use.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <returns>A collection of <see cref="FsmEvent"/> objects.</returns>
        public static IEnumerable<FsmEvent> GetEvents(this Fsm fsm, Func<FsmEvent, bool>? predicate = null)
        {
            fsm.ThrowIfNull(nameof(fsm));

            return (predicate is null) ? fsm.Events.AsEnumerable() : fsm.Events.Where(predicate);
        }

        #endregion

        #region Replace Methods

        /// <summary>
        /// Replaces the event in the <see cref="Fsm"/> that has the specified <paramref name="eventName"/> with the given <paramref name="replacementEvent"/>.
        /// </summary>
        /// <param name="eventName">The name of the event to replace.</param>
        ///
        /// <inheritdoc cref="ReplaceEvent(Fsm, Func{FsmEvent, bool}, FsmEvent)"/>
        public static bool ReplaceEvent(this Fsm fsm, string eventName, FsmEvent replacementEvent)
        {
            eventName.ThrowIfNull(nameof(eventName));

            return fsm.ReplaceEvent((evnt) => evnt.Name == eventName, replacementEvent);
        }

        /// <summary>
        /// Replaces the event at the specified <paramref name="eventIndex"/> in the <see cref="Fsm"/> with the given <paramref name="replacementEvent"/>.
        /// </summary>
        /// <param name="eventIndex">The index of the event in the <see cref="Fsm"/> to replace.</param>
        /// <returns/>
        ///
        /// <inheritdoc cref="ReplaceEvent(Fsm, Func{FsmEvent, bool}, FsmEvent)"/>
        /// <exception cref="IndexOutOfRangeException"/>
        public static void ReplaceEvent(this Fsm fsm, int eventIndex, FsmEvent replacementEvent)
        {
            fsm.ThrowIfNull(nameof(fsm));
            replacementEvent.ThrowIfNull(nameof(replacementEvent));

            fsm.Events[eventIndex] = replacementEvent;
        }

        /// <summary>
        /// Replaces the first event in the <see cref="Fsm"/> that is equal to <paramref name="targetEvent"/> with the given <paramref name="replacementEvent"/>.
        /// </summary>
        /// <param name="targetEvent">The event in the <see cref="Fsm"/> to replace.</param>
        ///
        /// <inheritdoc cref="ReplaceEvent(Fsm, Func{FsmEvent, bool}, FsmEvent)"/>
        public static bool ReplaceEvent(this Fsm fsm, FsmEvent targetEvent, FsmEvent replacementEvent)
        {
            targetEvent.ThrowIfNull(nameof(targetEvent));

            return fsm.ReplaceEvent((evnt) => evnt == targetEvent, replacementEvent);
        }

        /// <summary>
        /// Replaces the first event in the <see cref="Fsm"/> that matches the specified predicate with
        /// the given <paramref name="replacementEvent"/>.
        /// </summary>
        /// <param name="fsm">The FSM to look through.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <param name="replacementEvent">The event to replace the found event with.</param>
        /// <returns><see langword="true"/> if an item was successfully found and replaced; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static bool ReplaceEvent(this Fsm fsm, Func<FsmEvent, bool> predicate, FsmEvent replacementEvent)
        {
            fsm.ThrowIfNull(nameof(fsm));
            predicate.ThrowIfNull(nameof(predicate));
            replacementEvent.ThrowIfNull(nameof(replacementEvent));

            int targetIndex = Array.FindIndex(fsm.Events, (evnt) => predicate(evnt));
            if (targetIndex is -1)
                return false;

            fsm.Events[targetIndex] = replacementEvent;
            return true;
        }

        #endregion

        #region Add Methods

        /// <summary>
        /// Adds the specified event to the <see cref="Fsm"/>.
        /// </summary>
        /// <param name="fsm">The FSM to add to.</param>
        /// <param name="evnt">The event to add.</param>
        public static void AddEvent(this Fsm fsm, FsmEvent evnt)
        {
            fsm.ThrowIfNull(nameof(fsm));
            evnt.ThrowIfNull(nameof(evnt));

            fsm.Events = fsm.Events.Add(evnt);
        }

        /// <summary>
        /// Adds the specified events to the <see cref="Fsm"/>.
        /// </summary>
        /// <param name="fsm">The FSM to add to.</param>
        /// <param name="evnts">The events to add.</param>
        public static void AddEvents(this Fsm fsm, params FsmEvent[] evnts)
        {
            fsm.ThrowIfNull(nameof(fsm));
            evnts.ThrowIfNull(nameof(evnts));

            fsm.Events = fsm.Events.AddRange(evnts);
        }

        #endregion

        #region Insert Methods

        /// <summary>
        /// Inserts the specified event into the <see cref="Fsm"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="evnt">The event to insert.</param>
        ///
        /// <inheritdoc cref="InsertEvents(Fsm, int, FsmEvent[])"/>
        public static void InsertEvent(this Fsm fsm, int index, FsmEvent evnt)
        {
            fsm.ThrowIfNull(nameof(fsm));
            evnt.ThrowIfNull(nameof(evnt));

            fsm.Events = fsm.Events.Insert(index, evnt);
        }

        /// <summary>
        /// Inserts the specified events into the <see cref="Fsm"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="fsm">The FSM to insert into.</param>
        /// <param name="index">The index in the <see cref="Fsm"/> to insert at.</param>
        /// <param name="evnts">The events to insert.</param>
        public static void InsertEvents(this Fsm fsm, int index, params FsmEvent[] evnts)
        {
            fsm.ThrowIfNull(nameof(fsm));
            evnts.ThrowIfNull(nameof(evnts));

            fsm.Events = fsm.Events.InsertRange(index, evnts);
        }

        #endregion

        #region Remove Methods

        /// <summary>
        /// Removes the first event with the specified <paramref name="eventName"/> from the <see cref="Fsm"/>.
        /// </summary>
        /// <param name="eventName">The name of the event to remove.</param>
        ///
        /// <inheritdoc cref="RemoveEvents(Fsm, Func{FsmEvent, bool})"/>
        public static bool RemoveEvent(this Fsm fsm, string eventName)
        {
            return fsm.RemoveEvent((evnt) => evnt.Name == eventName);
        }

        /// <summary>
        /// Removes the event at the specified <paramref name="eventIndex"/> from the <see cref="Fsm"/>.
        /// </summary>
        /// <param name="eventIndex">The index of the event to remove.</param>
        /// <returns/>
        ///
        /// <inheritdoc cref="RemoveEvents(Fsm, Func{FsmEvent, bool})"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static void RemoveEvent(this Fsm fsm, int eventIndex)
        {
            fsm.ThrowIfNull(nameof(fsm));

            fsm.Events = fsm.Events.RemoveAt(eventIndex);
        }

        /// <summary>
        /// Removes the specified event from the <see cref="Fsm"/>.
        /// </summary>
        /// <param name="targetEvent">The event to remove.</param>
        ///
        /// <inheritdoc cref="RemoveEvents(Fsm, Func{FsmEvent, bool})"/>
        public static bool RemoveEvent(this Fsm fsm, FsmEvent targetEvent)
        {
            targetEvent.ThrowIfNull(nameof(targetEvent));

            return fsm.RemoveEvent((evnt) => evnt == targetEvent);
        }

        /// <summary>
        /// Removes the first event that matches the specified <paramref name="predicate"/> from the <see cref="Fsm"/>.
        /// </summary>
        ///
        /// <inheritdoc cref="RemoveEvents(Fsm, Func{FsmEvent, bool})"/>
        public static bool RemoveEvent(this Fsm fsm, Func<FsmEvent, bool> predicate)
        {
            fsm.ThrowIfNull(nameof(fsm));
            predicate.ThrowIfNull(nameof(predicate));

            var withoutEvent = fsm.Events.WithoutFirst(predicate).ToArray();
            bool wasRemoved = withoutEvent.Length != fsm.Events.Length;

            fsm.Events = withoutEvent;
            return wasRemoved;
        }

        /// <summary>
        /// Removes the specified events from the <see cref="Fsm"/>.
        /// </summary>
        /// <param name="targetEvents">The events to remove.</param>
        ///
        /// <inheritdoc cref="RemoveEvents(Fsm, Func{FsmEvent, bool})"/>
        public static bool RemoveEvents(this Fsm fsm, params FsmEvent[] targetEvents)
        {
            targetEvents.ThrowIfNull(nameof(targetEvents));

            return fsm.RemoveEvents((evnt) => targetEvents.Contains(evnt));
        }

        /// <summary>
        /// Removes the events from the <see cref="Fsm"/> that match the specified <paramref name="predicate"/>.
        /// </summary>
        /// <param name="fsm">The FSM to remove from.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <returns><see langword="true"/> if an item was successfully found and removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static bool RemoveEvents(this Fsm fsm, Func<FsmEvent, bool> predicate)
        {
            fsm.ThrowIfNull(nameof(fsm));
            predicate.ThrowIfNull(nameof(predicate));

            var withoutEvents = fsm.Events.Where((evnt) => !predicate(evnt)).ToArray();
            bool anyRemoved = withoutEvents.Length != fsm.Events.Length;

            fsm.Events = withoutEvents;
            return anyRemoved;
        }

        #endregion
    }
}