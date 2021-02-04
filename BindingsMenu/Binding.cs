#nullable enable
#pragma warning disable IDE0051 // Remove unused private members

using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace TogglableBindings
{
    [JsonObject]
    public abstract class Binding
    {
        private const string MustBeNearBench = "Must be near a bench to {0} this binding.";

        public event BindingEventHandler? Applied;

        public event BindingEventHandler? Restored;

        /// <summary>
        /// Gets the name of this binding.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets whether this binding is currently applied.
        /// </summary>
        [JsonIgnore]
        public bool IsApplied { get; private set; }

        [JsonProperty]
        internal bool WasApplied { get; private set; }

        /// <summary>
        /// Creates a new binding with the specified name.
        /// </summary>
        /// <param name="name">The name of the binding.</param>
        public Binding(string name)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        /// <summary>
        /// Checks to see whether this binding should currently be able to be applied.
        /// Note that this does not prevent <see cref="Apply"/> from being called; this
        /// is more to check if a player should be prevented from enabling the binding.
        /// <br/>
        /// The binding may, at any time, be applied regardless of the
        /// result of this method.
        /// <para/>
        /// Base behavior is to prevent the binding from being applied when the player is
        /// not near a bench.
        /// <br/>
        /// '<c>HeroController.instance?.cState?.nearBench ?? false</c>'
        /// </summary>
        /// <returns>
        /// A <see cref="ResultInfo{T}"/> of type <see cref="bool"/> containing:
        /// <br/>
        /// • <see langword="false"/> if the player should currently be prevented from applying
        /// this binding, along with a descriptive, user-facing string indicating the reason.
        /// <br/>
        /// • <see langword="true"/> otherwise.
        /// </returns>
        public virtual ResultInfo<bool> CanBeApplied()
        {
            bool isNearBench = HeroController.instance?.cState?.nearBench ?? false;
            return new(isNearBench, string.Format(MustBeNearBench, "apply"));
        }

        /// <summary>
        /// Checks to see whether this binding should currently be able to be restored.
        /// Note that this does not prevent <see cref="Restore"/> from being called; this
        /// is more to check if a player should be prevented from disabling the binding.
        /// <br/>
        /// The binding may, at any time, be restored regardless of the
        /// result of this method, such as when a save is exited.
        /// <para/>
        /// Base behavior is to prevent the binding from being restored when the player is
        /// not near a bench.
        /// <br/>
        /// '<c>HeroController.instance?.cState?.nearBench ?? false</c>'
        /// </summary>
        /// <returns>
        /// A <see cref="ResultInfo{T}"/> of type <see cref="bool"/> containing:
        /// <br/>
        /// • <see langword="false"/> if the player should currently be prevented from restoring
        /// this binding, along with a descriptive, user-facing string indicating the reason.
        /// <br/>
        /// • <see langword="true"/> otherwise.
        /// </returns>
        public virtual ResultInfo<bool> CanBeRestored()
        {
            bool isNearBench = HeroController.instance?.cState?.nearBench ?? false;
            return new(isNearBench, string.Format(MustBeNearBench, "restore"));
        }

        /// <summary>
        /// Applies this binding, enabling its effects.
        /// </summary>
        public void Apply()
        {
            if (IsApplied)
                return;

            TogglableBindings.Instance.LogDebug(Name + " applied");

            OnApplied();
            Applied?.Invoke(this);
            IsApplied = true;
        }

        /// <summary>
        /// Restores this binding, disabling its effects.
        /// </summary>
        public void Restore()
        {
            if (!IsApplied)
                return;

            TogglableBindings.Instance.LogDebug(Name + " restored");

            OnRestored();
            Restored?.Invoke(this);
            IsApplied = false;
        }

        /// <summary>
        /// Executed when <see cref="Apply"/> is called.
        /// </summary>
        protected abstract void OnApplied();

        /// <summary>
        /// Executed when <see cref="Restore"/> is called.
        /// </summary>
        protected abstract void OnRestored();

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            WasApplied = IsApplied;
        }
    }
}