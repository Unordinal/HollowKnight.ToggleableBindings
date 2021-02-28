#nullable enable

using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using ToggleableBindings.Utility;
using ToggleableBindings.VanillaBindings;
using UnityEngine;
using TB = ToggleableBindings.ToggleableBindings;

namespace ToggleableBindings
{
    /// <summary>
    /// Represents a binding - a modifier that changes the game in some way when active.
    /// <para>
    /// The members of a binding are serialized to the save settings file on an opt-in basis.
    /// If you want a non-static field or property to be saved and loaded with a save file,
    /// mark it with <see cref="JsonPropertyAttribute"/>.
    /// </para>
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Binding
    {
        internal static Sprite UnknownDefault => EmbeddedAssetLoader.UnknownBindingDefault;

        internal static Sprite UnknownSelected => EmbeddedAssetLoader.UnknownBindingSelected;

        private const string MustBeNearBench = "Must be near a bench to {0} this binding."; // {0} = 'apply', 'restore'

        /// <summary>
        /// Invoked when this binding is applied.
        /// </summary>
        public event BindingEventHandler? Applied;

        /// <summary>
        /// Invoked when this binding is restored.
        /// </summary>
        public event BindingEventHandler? Restored;

        /// <summary>
        /// Gets the ID of this binding. Output is of the format '<c>Assembly::BindingTypeName</c>'.
        /// <para/>
        /// Example: <c>ToggleableBindings::<see cref="NailBinding"/></c>
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// Gets the display name of this binding.
        /// This is automatically forced into uppercase when displayed in the bindings menu.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets whether this binding is currently applied.
        /// </summary>
        public bool IsApplied { get; private set; }

        /// <summary>
        /// Gets whether this binding is considered a base game binding.
        /// </summary>
        public bool IsVanillaBinding { get; }

        /// <summary>
        /// Gets the sprite used for the binding button's default state in the UI.
        /// <br/>
        /// If <see langword="null"/>, uses a default placeholder.
        /// </summary>
        public virtual Sprite? DefaultSprite { get; protected set; }

        /// <summary>
        /// Gets the sprite used for the binding button's selected state in the UI.
        /// <br/>
        /// If <see langword="null"/>, uses a default placeholder.
        /// </summary>
        public virtual Sprite? SelectedSprite { get; protected set; }

        /// <summary>
        /// Gets whether this binding was applied when a save was saved. Can be used to adjust
        /// <see cref="OnApplied"/> accordingly if needed.
        /// This property is set when the binding is serialized.
        /// </summary>
        [JsonProperty(Order = -5)]
        protected internal bool WasApplied { get; protected set; }

        /// <summary>
        /// Creates a new binding with the specified name.
        /// </summary>
        /// <param name="name">The name of the binding.</param>
        public Binding(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            Type bindingType = GetType();
            string asmName = bindingType.Assembly.GetName().Name;

            ID = $"{asmName}::{bindingType.Name}";
            Name = name;
            if (IsVanilla(bindingType)) // Vanilla bindings have VanillaBindingAttribute applied to them.
                IsVanillaBinding = true;
        }

        /// <summary>
        /// Checks to see whether this binding should currently be able to be applied.
        /// Note that this does not directly prevent <see cref="Apply"/> from being called; this
        /// is more to check if a player should be prevented from enabling the binding.
        /// <br/>
        /// The binding may, at any time, be applied regardless of the
        /// result of this method. If a player sets the setting <see cref="TB.EnforceBindingRestrictions"/>
        /// to <see langword="false"/>, it will stop this method from being called as well.
        /// <para>
        /// Base behavior is to return <see langword="true"/> when the player is
        /// within 10 units of a bench and <see langword="false"/> otherwise.
        /// </para>
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
            bool isNearBench = false;
            var bench = ObjectUtil.GetBenchInScene();
            if (bench != null)
                isNearBench = ObjectUtil.HeroIsWithinDistanceOf(bench, 10f);

            return new(isNearBench, string.Format(MustBeNearBench, "apply"));
        }

        /// <summary>
        /// Checks to see whether this binding should currently be able to be restored.
        /// Note that this does not directly prevent <see cref="Restore"/> from being called; this
        /// is more to check if a player should be prevented from disabling the binding.
        /// <br/>
        /// The binding may, at any time, be restored regardless of the
        /// result of this method, such as when a save is exited. If a player sets the setting <see cref="TB.EnforceBindingRestrictions"/>
        /// to <see langword="false"/>, it will stop this method from being called as well.
        /// <para>
        /// Base behavior is to return <see langword="true"/> when the player is
        /// within 10 units of a bench and <see langword="false"/> otherwise.
        /// </para>
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
            bool isNearBench = false;
            var bench = ObjectUtil.GetBenchInScene();
            if (bench != null)
                isNearBench = ObjectUtil.HeroIsWithinDistanceOf(bench, 10f);

            return new(isNearBench, string.Format(MustBeNearBench, "restore"));
        }

        /// <summary>
        /// Applies this binding, enabling its effects.
        /// <para>
        /// Bindings should be registered with <see cref="BindingManager"/> before
        /// being applied or restored.
        /// </para>
        /// </summary>
        public void Apply()
        {
            if (IsApplied)
                return;

            IsApplied = true;
            OnApplied();
            Applied?.Invoke(this);
        }

        /// <summary>
        /// Restores this binding, disabling its effects.
        /// <para><inheritdoc cref="Apply"/></para>
        /// </summary>
        public void Restore()
        {
            if (!IsApplied)
                return;

            IsApplied = false;
            OnRestored();
            Restored?.Invoke(this);
        }

        /// <summary>
        /// Executed when <see cref="Apply"/> is called only if the binding is not applied.
        /// <see cref="Applied"/> is invoked after this method is called.
        /// </summary>
        protected abstract void OnApplied();

        /// <summary>
        /// Executed when <see cref="Restore"/> is called only if the binding is applied.
        /// <see cref="Restored"/> is invoked after this method is called.
        /// <para>
        /// <list type="number">
        ///     <listheader>
        ///         <term>IMPORTANT</term>
        ///         <description>This method should have two guarantees:</description>
        ///     </listheader>
        ///     <item>
        ///         <term>Fresh state</term> 
        ///         <description>This means that the binding should be put into a 'just initialized' state.</description>
        ///     </item>
        ///     <item>
        ///         <term>Finish before game save</term>
        ///         <description>
        ///             Any work this method does that touches player data which is saved to the normal game save file should be finished before that file is saved.
        ///             Even a single 'yield return null' coroutine will cause this to fail.
        ///         </description>
        ///     </item>
        /// </list>
        /// Failure to respect either one of these guarantees will cause issues in some circumstances.
        /// </para>
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

        private static bool IsVanilla(Type bindingType)
        {
            return bindingType.IsDefined(typeof(VanillaBindingAttribute), false);
        }
    }
}