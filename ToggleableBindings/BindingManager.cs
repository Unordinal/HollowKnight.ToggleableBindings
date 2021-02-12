#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Modding;
using ToggleableBindings.HKQuickSettings;
using ToggleableBindings.VanillaBindings;
using UnityEngine;

namespace ToggleableBindings
{
    public static class BindingManager
    {
        /// <summary>
        /// Invoked when a binding is successfully registered.
        /// </summary>
        public static event BindingEventHandler? BindingRegistered;
        /// <summary>
        /// Invoked when a binding is successfully deregistered.
        /// </summary>
        public static event BindingEventHandler? BindingDeregistered;
        /// <summary>
        /// Invoked when a binding is applied (enabled).
        /// </summary>
        public static event BindingEventHandler? BindingApplied;
        /// <summary>
        /// Invoked when a binding is restored (disabled).
        /// </summary>
        public static event BindingEventHandler? BindingRestored;

        [QuickSetting(true, nameof(RegisteredBindings))]
        private static List<Binding> _serializableBindings = new(0);

        private static readonly List<Binding> _registeredBindings = new(4);

        /// <summary>
        /// Gets a read-only collection that contains the currently registered bindings.
        /// </summary>
        public static ReadOnlyCollection<Binding> RegisteredBindings { get; } = new(_registeredBindings);

        internal static void Initialize()
        {
            RegisterVanillaBindings();
        }

        internal static void Unload(bool waitForSaveFinish = true)
        {
            foreach (var binding in RegisteredBindings)
            {
                binding.Restore();

                binding.Applied -= OnBindingApplied;
                binding.Restored -= OnBindingRestored;
            }

            if (waitForSaveFinish)
                ModHooks.Instance.SavegameSaveHook += ClearRegistered;
            else
                _registeredBindings.Clear();

            static void ClearRegistered(int slotID = -1)
            {
                _registeredBindings.Clear();
                ModHooks.Instance.SavegameSaveHook -= ClearRegistered;
            }
        }

        /// <summary>
        /// Applies the specified registered binding, enabling its effects.
        /// This will throw if the specified binding is not registered, and is
        /// equivalent to calling <see cref="Binding.Apply"/> otherwise.
        /// </summary>
        /// <param name="binding">The binding to apply.</param>
        /// <exception cref="InvalidOperationException"/>
        public static void ApplyBinding(Binding binding)
        {
            EnsureBindingRegistered(binding);
            binding.Apply();
        }

        /// <summary>
        /// Restores the specified registered binding, removing its effects.
        /// This will throw if the specified binding is not registered, and is
        /// equivalent to calling <see cref="Binding.Restore"/> otherwise.
        /// </summary>
        /// <param name="binding">The binding to restore.</param>
        /// <exception cref="InvalidOperationException"/>
        public static void RestoreBinding(Binding binding)
        {
            EnsureBindingRegistered(binding);
            binding.Restore();
        }

        /// <summary>
        /// Checks to see if the specified <see cref="Binding"/> is already registered.
        /// </summary>
        /// <param name="binding">The binding to check the registration state of.</param>
        /// <returns><see langword="true"/> if <paramref name="binding"/> is registered; otherwise, <see langword="false"/>.</returns>
        public static bool IsBindingRegistered(Binding binding)
        {
            return RegisteredBindings.Contains(binding);
        }

        /// <summary>
        /// Register the specified binding, adding it to the <see cref="RegisteredBindings"/> list and subscribing it to
        /// the binding applied/restored events before invoking <see cref="BindingRegistered"/>.
        /// </summary>
        /// <param name="binding">The binding to register. The binding must not already be registered.</param>
        /// <exception cref="InvalidOperationException">The given binding was already registered.</exception>
        public static void RegisterBinding(Binding binding)
        {
            if (IsBindingRegistered(binding))
                throw new InvalidOperationException("The specified binding is already registered.");

            OnBindingRegistered(binding);
        }

        /// <summary>
        /// Deregister the specified binding, calling <see cref="Binding.Restore"/> on it and then removing it from <see cref="RegisteredBindings"/> and unsubscribing it from the binding
        /// applied/restored events before invoking <see cref="BindingDeregistered"/>.
        /// <para/>
        /// Note: You cannot deregister a binding that is part of the base game - the following types are base game bindings:
        /// <br>• <see cref="NailBinding"/></br>
        /// <br>• <see cref="ShellBinding"/></br>
        /// <br>• <see cref="SoulBinding"/></br>
        /// <br>• <see cref="CharmsBinding"/></br>
        /// </summary>
        /// <param name="binding">The binding to deregister.</param>
        public static void DeregisterBinding(Binding binding)
        {
            if (!IsBindingRegistered(binding))
                return;

            if (binding.IsVanillaBinding)
                throw new ArgumentException("Cannot deregister a base game binding.");

            OnBindingDeregistered(binding);
        }

        private static void ForceDeregisterBinding(Binding binding)
        {
            binding.Applied -= OnBindingApplied;
            binding.Restored -= OnBindingRestored;

            _registeredBindings.Remove(binding);
        }

        private static void OnBindingRegistered(Binding binding)
        {
            binding.Applied += OnBindingApplied;
            binding.Restored += OnBindingRestored;
            _registeredBindings.Add(binding);

            BindingRegistered?.Invoke(binding);
        }

        private static void OnBindingDeregistered(Binding binding)
        {
            binding.Applied -= OnBindingApplied;
            binding.Restored -= OnBindingRestored;
            _registeredBindings.Remove(binding);

            BindingDeregistered?.Invoke(binding);
        }

        private static void OnBindingApplied(Binding binding)
        {
            BindingApplied?.Invoke(binding);
        }

        private static void OnBindingRestored(Binding binding)
        {
            BindingRestored?.Invoke(binding);
        }

        private static void RegisterVanillaBindings()
        {
            RegisterBinding(new NailBinding());
            RegisterBinding(new ShellBinding());
            RegisterBinding(new SoulBinding());
            RegisterBinding(new CharmsBinding());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binding"></param>
        /// <exception cref="InvalidOperationException"/>
        private static void EnsureBindingRegistered(Binding binding)
        {
            if (!IsBindingRegistered(binding))
                throw new InvalidOperationException("The specified binding isn't registered.");
        }

        private static void OnSerializing()
        {
            ToggleableBindings.Instance.LogDebug(nameof(BindingManager) + " OnSerializing");
            _serializableBindings = _registeredBindings;
        }

        private static void OnDeserialized()
        {
            ToggleableBindings.Instance.LogDebug(nameof(BindingManager) + " OnDeserialized");

            CoroutineController.Start(RegisterDeserializedBindings());
        }

        private static IEnumerator RegisterDeserializedBindings()
        {
            foreach (var binding in _serializableBindings)
            {
                if (!IsBindingRegistered(binding))
                    RegisterBinding(binding);

                if (binding.WasApplied)
                    ApplyBinding(binding);

                yield return null;
            }
        }
    }
}