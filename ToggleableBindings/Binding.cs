#nullable enable

using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using ToggleableBindings.VanillaBindings;

namespace ToggleableBindings
{
    /// <summary>
    /// Represents a binding - a modifier that changes the difficulty of the game when active.
    /// </summary>
    [JsonObject]
    public abstract class Binding
    {
        private const string MustBeNearBench = "Must be near a bench to {0} this binding."; // {0} = 'apply', 'restore'

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

        /// <summary>
        /// Gets whether this binding is a base game binding.
        /// </summary>
        [JsonIgnore]
        public bool IsVanillaBinding { get; }

        /// <summary>
        /// Gets whether this binding was applied when a save was saved. Can be used to adjust
        /// <see cref="OnApplied"/> accordingly.
        /// This property is set when the binding is serialized.
        /// </summary>
        [JsonProperty]
        protected internal bool WasApplied { get; protected set; }

        /// <summary>
        /// Creates a new binding with the specified name.
        /// </summary>
        /// <param name="name">The name of the binding.</param>
        public Binding(string name)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            Name = name;
            if (IsVanilla(GetType())) // Derived class has VanillaBindingAttribute applied to it.
                IsVanillaBinding = true;
        }

        /// <summary>
        /// Checks to see whether this binding should currently be able to be applied.
        /// Note that this does not directly prevent <see cref="Apply"/> from being called; this
        /// is more to check if a player should be prevented from enabling the binding.
        /// <br/>
        /// The binding may, at any time, be applied regardless of the
        /// result of this method.
        /// <para/>
        /// Base behavior is to return <see langword="false"/> when the player is
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
        /// Note that this does not directly prevent <see cref="Restore"/> from being called; this
        /// is more to check if a player should be prevented from disabling the binding.
        /// <br/>
        /// The binding may, at any time, be restored regardless of the
        /// result of this method, such as when a save is exited.
        /// <para/>
        /// Base behavior is to return <see langword="false"/> when the player is
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

            ToggleableBindings.Instance.LogDebug($"Binding '{Name}' applied.");

            IsApplied = true;
            OnApplied();
            Applied?.Invoke(this);
        }

        /// <summary>
        /// Restores this binding, disabling its effects.
        /// </summary>
        public void Restore()
        {
            if (!IsApplied)
                return;

            ToggleableBindings.Instance.LogDebug($"Binding '{Name}' restored.");

            IsApplied = false;
            OnRestored();
            Restored?.Invoke(this);
        }

        /// <summary>
        /// Executed when <see cref="Apply"/> is called only if the binding was not already applied.
        /// <see cref="Applied"/> is invoked after this method is called.
        /// </summary>
        protected abstract void OnApplied();

        /// <summary>
        /// Executed when <see cref="Restore"/> is called only if the binding was previously applied.
        /// <see cref="Restored"/> is invoked after this method is called.
        /// </summary>
        protected abstract void OnRestored();

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            WasApplied = IsApplied;
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext context)
        {
            WasApplied = false;
        }

        internal static bool IsVanilla(Type bindingType)
        {
            return bindingType.IsDefined(typeof(VanillaBindingAttribute), false);
        }
    }
}