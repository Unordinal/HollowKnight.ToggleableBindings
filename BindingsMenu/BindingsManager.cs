#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Modding;
using Newtonsoft.Json;
using TogglableBindings.HKQuickSettings;
using TogglableBindings.VanillaBindings;
using UnityEngine;
using Logger = Modding.Logger;

namespace TogglableBindings
{
    public static class BindingsManager
    {
        public static event BindingEventHandler? BindingRegistered;
        public static event BindingEventHandler? BindingDeregistered;
        public static event BindingEventHandler? BindingApplied;
        public static event BindingEventHandler? BindingRestored;

        [QuickSetting(true, nameof(RegisteredBindings))]
        private static List<Binding> _serializableBindings = new(0);

        private static List<Binding> _registeredBindings = new(4);

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
        /// </summary>
        /// <param name="binding">The binding to apply.</param>
        public static void ApplyBinding(Binding binding)
        {
            EnsureBindingRegistered(binding);

            if (binding.IsApplied)
                return;

            binding.Apply();
        }

        /// <summary>
        /// Restores the specified registered binding, removing its effects.
        /// </summary>
        /// <param name="binding">The binding to restore.</param>
        public static void RestoreBinding(Binding binding)
        {
            EnsureBindingRegistered(binding);

            if (!binding.IsApplied)
                return;

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
        /// </summary>
        /// <param name="binding">The binding to deregister.</param>
        public static void DeregisterBinding(Binding binding)
        {
            if (!IsBindingRegistered(binding))
                return;

            OnBindingDeregistered(binding);
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

        private static void EnsureBindingRegistered(Binding binding)
        {
            if (!IsBindingRegistered(binding))
                throw new InvalidOperationException("The specified binding isn't registered!");
        }

        private static void OnSerializing()
        {
            TogglableBindings.Instance.LogDebug(nameof(BindingsManager) + " OnSerializing");
            _serializableBindings = _registeredBindings;
        }

        private static void OnDeserialized()
        {
            TogglableBindings.Instance.LogDebug(nameof(BindingsManager) + " OnDeserialized");
            foreach (var binding in RegisteredBindings.ToArray())
                DeregisterBinding(binding);

            CoroutineController.Start(RegisterSerializedBindings());
        }

        private static IEnumerator RegisterSerializedBindings()
        {
            yield return new WaitWhile(() => HeroController.instance is null);
            yield return null;

            foreach (var binding in _serializableBindings)
            {
                RegisterBinding(binding);
                if (binding.WasApplied)
                    ApplyBinding(binding);

                yield return null;
            }
        }
    }
}