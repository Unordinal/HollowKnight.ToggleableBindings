#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToggleableBindings.Collections;
using ToggleableBindings.Exceptions;
using ToggleableBindings.Extensions;
using ToggleableBindings.HKQuickSettings;
using ToggleableBindings.VanillaBindings;
using UnityEngine;

using TB = ToggleableBindings.ToggleableBindings;

namespace ToggleableBindings
{
    public static class BindingManager
    {
        private const string ExcTypeIsBaseBinding = "The specified type parameter must not be equal to 'typeof(" + nameof(Binding) + ")'.";
        private const string ExcTypeNotRegistered = "A binding of the specified type is not registered.";

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
        private static List<Binding> _serializedBindings = new(0);

        private static readonly Dictionary<Type, Binding> _registeredBindings = new(4);

        /// <summary>
        /// Gets a read-only dictionary that provides a view of the currently registered binding types and their associated binding objects.
        /// </summary>
        public static IReadOnlyDictionary<Type, Binding> RegisteredBindings { get; } = new ReadOnlyDictionary<Type, Binding>(_registeredBindings);

        internal static void Initialize()
        {
            AddHooks();
            RegisterVanillaBindings();
        }

        internal static void Unload()
        {
            RemoveHooks();
            RestoreAllAndClear();
        }

        private static void AddHooks()
        {
            TB.MainMenuOrQuit += CleanUpForSave;
        }

        private static void RemoveHooks()
        {
            TB.MainMenuOrQuit -= CleanUpForSave;
        }

        private static void CleanUpForSave()
        {
            /*
             * There's probably a cleaner/better way to do this, but I can't think of it right now.
             *
             * We need to ensure that the bindings are restored before actual game data is saved
             * in case the mod is removed before the next load and one or more bindings affected
             * the player character's stats or similar.
             *
             * We only want to do this if we're not staying in the save file to continue playing,
             * so we hook the MainMenuOrQuit event first, and THEN add a temporary hook
             * on SaveSettingsSaved.
             *
             * The necessary order:
             * - SaveIsBeingQuit -> SettingsSaved -> BindingsRestored -> SaveDataSaved -> Quit
             *
             * Hooking MainMenuOrQuit alone will break the order as then this would happen before
             * the settings were saved.
             *
             * This involves checking to see if we're going back to the main menu or the
             * application is closed and then hooking the SaveSettingsSaved (which happens right
             * before the actual game data is saved) and restoring/deregistering all of the bindings
             * after the save settings are serialized.
             *
             * This ensures that their serializable properties are written to file before we reset them.
             */

            Action<int>? handler = null;
            TB.Instance.Settings.SaveSettingsSaved += handler = (saveSlot) =>
            {
                TB.Instance.Settings.SaveSettingsSaved -= handler;

                RestoreAllAndClear();
            };
        }

        private static void RestoreAllAndClear()
        {
            foreach (var binding in _registeredBindings.Values)
            {
                binding.Restore();
                binding.Applied -= OnBindingApplied;
                binding.Restored -= OnBindingRestored;
            }

            _registeredBindings.Clear();
        }

        /// <summary>
        /// Checks to see if the specified type of binding is already registered.
        /// </summary>
        /// <param name="bindingType">The type of binding to check the registration state of. Must not be equal to <c>typeof(<see cref="Binding"/>)</c>.</param>
        /// <returns><see langword="true"/> if a binding of the given type is registered; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public static bool IsBindingRegistered(Type bindingType)
        {
            EnsureTypeIsNotBaseBinding(bindingType, nameof(bindingType));

            return RegisteredBindings.ContainsKey(bindingType);
        }

        /// <typeparam name="T">The type of binding. <c>typeof(<typeparamref name="T"/>)</c> must not be equal to <c>typeof(<see cref="Binding"/>)</c>.</typeparam>
        /// <inheritdoc cref="IsBindingRegistered(Type)" path="//*[not(self::exception)]"/>
        /// <exception cref="TypeArgumentException"/>
        public static bool IsBindingRegistered<T>() where T : Binding
        {
            EnsureTypeIsNotBaseBinding<T>(nameof(T));

            return RegisteredBindings.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Gets the registered binding with the specified name. If a binding with the specified name
        /// is not registered, returns <see langword="null"/>.
        /// </summary>
        /// <param name="bindingName">The name of the binding to retrieve.</param>
        /// <returns>The found <see cref="Binding"/> if successful; otherwise, <see langword="null"/>.</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public static Binding? GetRegisteredBinding(string bindingName)
        {
            if (bindingName is null or "Binding")
                return null;

            Binding? result = RegisteredBindings.Values.FirstOrDefault(b => b.Name == bindingName);
            if (result is null)
                TB.Instance.LogDebug($"Attempted to get binding '{bindingName}' but a binding with that name is not registered.");

            return result;
        }
        
        /// <summary>
        /// Gets the registered binding of the specified type. If a binding of the specified type
        /// is not registered, returns <see langword="null"/>.
        /// </summary>
        /// <param name="bindingType">The type of the binding to retrieve. Must not be equal to <c>typeof(<see cref="Binding"/>)</c>.</param>
        /// <returns>The found <see cref="Binding"/> if successful; otherwise, <see langword="null"/>.</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public static Binding? GetRegisteredBinding(Type bindingType)
        {
            EnsureTypeIsNotBaseBinding(bindingType, nameof(bindingType));

            RegisteredBindings.TryGetValue(bindingType, out Binding? result);
            if (result is null)
                TB.Instance.LogDebug($"Attempted to get binding '{bindingType.Name}' but a binding of that type is not registered.");

            return result;
        }

        /// <typeparam name="T">The type of the binding to retrieve.</typeparam>
        /// <inheritdoc cref="GetRegisteredBinding(Type)" path="/*[not(self::exception)]"/>
        /// <exception cref="TypeArgumentException"/>
        public static T? GetRegisteredBinding<T>() where T : Binding
        {
            EnsureTypeIsNotBaseBinding<T>(nameof(T));

            RegisteredBindings.TryGetValue(typeof(T), out Binding? result);
            if (result is null)
                TB.Instance.LogDebug($"Attempted to get binding '{typeof(T).Name}' but a binding of that type is not registered.");

            return (T?)result;
        }

        #region RegisterBinding

        /// <summary>
        /// Register the specified binding, adding it to the <see cref="RegisteredBindings"/> list and subscribing it to
        /// the binding applied/restored events before invoking <see cref="BindingRegistered"/>.
        /// <para/>
        /// Only one binding of a certain type can be registered at a time.
        /// </summary>
        /// <param name="binding">The binding to register. The binding must not already be registered.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException">The given type of binding was already registered.</exception>
        public static void RegisterBinding(Binding binding)
        {
            if (binding is null)
                throw new ArgumentNullException(nameof(binding));

            if (IsBindingRegistered(binding.GetType()))
                throw new InvalidOperationException("A binding of the same type is already registered.");

            OnBindingRegistered(binding);
        }

        /// <summary>
        /// Register a new binding of the specified type, adding it to the <see cref="RegisteredBindings"/> list and subscribing it to
        /// the binding applied/restored events before invoking <see cref="BindingRegistered"/>.
        /// <para/>
        /// Only one binding of a certain type can be registered at a time.
        /// </summary>
        /// <typeparam name="T">The type of binding to register. A binding of this type must not already be registered.</typeparam>
        /// <exception cref="InvalidOperationException">The given type of binding was already registered.</exception>
        public static void RegisterBinding<T>() where T : Binding, new()
        {
            if (IsBindingRegistered<T>())
                throw new InvalidOperationException("A binding of the same type is already registered.");

            OnBindingRegistered(new T());
        }

        #endregion

        #region DeregisterBinding

        /// <summary>
        /// Deregister the specified binding, calling <see cref="Binding.Restore"/> on it and
        /// then removing it from <see cref="RegisteredBindings"/> and unsubscribing it from the binding
        /// applied/restored events before invoking <see cref="BindingDeregistered"/>.
        /// <para>
        /// Note: You cannot deregister a binding that is part of the base game - the following types are base game bindings:
        /// <br>• <see cref="NailBinding"/></br>
        /// <br>• <see cref="ShellBinding"/></br>
        /// <br>• <see cref="SoulBinding"/></br>
        /// <br>• <see cref="CharmsBinding"/></br>
        /// </para>
        /// </summary>
        /// <param name="binding">The binding to deregister.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public static void DeregisterBinding(Binding binding)
        {
            if (binding is null)
                throw new ArgumentNullException(nameof(binding));

            DeregisterBinding(binding.GetType());
        }

        /// <summary>
        /// Deregister the binding of the specified type, calling <see cref="Binding.Restore"/> on it and
        /// then removing it from <see cref="RegisteredBindings"/> and unsubscribing it from the binding
        /// applied/restored events before invoking <see cref="BindingDeregistered"/>.
        /// <para><inheritdoc cref="DeregisterBinding(Binding)"/></para>
        /// </summary>
        /// <param name="bindingType">The type of the binding to deregister. Must not be equal to <c>typeof(<see cref="Binding"/>)</c>.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public static void DeregisterBinding(Type bindingType)
        {
            if (bindingType is null)
                throw new ArgumentNullException(nameof(bindingType));

            if (!IsBindingRegistered(bindingType))
                return;

            if (Binding.IsVanilla(bindingType))
                throw new ArgumentException("Cannot deregister a base game binding.");

            OnBindingDeregistered(RegisteredBindings[bindingType]);
        }

        /// <typeparam name="T"><inheritdoc cref="DeregisterBinding(Type)" path="/param[1]"/></typeparam>
        /// <inheritdoc cref="DeregisterBinding(Type)" path="/*[not(self::exception)]"/>
        /// <exception cref="TypeArgumentException"/>
        public static void DeregisterBinding<T>() where T : Binding
        {
            DeregisterBinding(typeof(T));
        }

        #endregion

        #region ApplyBinding

        /// <summary>
        /// Applies the specified registered binding, enabling its effects.
        /// This will throw if the specified binding is not registered, and is
        /// equivalent to calling <see cref="Binding.Apply"/> otherwise.
        /// </summary>
        /// <param name="binding">The binding to apply.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        public static void ApplyBinding(Binding binding)
        {
            EnsureBindingRegistered(binding);
            binding.Apply();
        }

        /// <summary>
        /// Applies the registered binding of the specified type, enabling its effects.
        /// This will throw if a binding of the specified type is not registered.
        /// </summary>
        /// <param name="bindingType">The binding of the specified type to apply. Must not be <c>typeof(<see cref="Binding"/>)</c>.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        public static void ApplyBinding(Type bindingType)
        {
            EnsureTypeIsNotBaseBinding(bindingType, nameof(bindingType));

            Binding? binding = GetRegisteredBinding(bindingType);
            if (binding is null)
                throw new InvalidOperationException(ExcTypeNotRegistered);

            binding.Apply();
        }

        /// <typeparam name="T">The binding of the specified type to apply. Must not be <c>typeof(<see cref="Binding"/>)</c>.</typeparam>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="TypeArgumentException"/>
        /// <inheritdoc cref="ApplyBinding(Type)" path="/*[not(self::exception)]"/>
        public static void ApplyBinding<T>() where T : Binding
        {
            Binding? binding = GetRegisteredBinding<T>();
            if (binding is null)
                throw new InvalidOperationException(ExcTypeNotRegistered);

            binding.Apply();
        }

        #endregion

        #region RestoreBinding

        /// <summary>
        /// Restores the specified registered binding, removing its effects.
        /// This will throw if the specified binding is not registered, and is
        /// equivalent to calling <see cref="Binding.Restore"/> otherwise.
        /// </summary>
        /// <param name="binding">The binding to restore.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        public static void RestoreBinding(Binding binding)
        {
            EnsureBindingRegistered(binding);

            binding.Restore();
        }

        /// <summary>
        /// Restores the registered binding of the specified type, removing its effects.
        /// This will throw if a binding of the specified type is not registered.
        /// </summary>
        /// <param name="bindingType">The binding of the specified type to restore.</param>
        /// <exception cref="ArgumentException"/>
        /// <inheritdoc cref="RestoreBinding(Binding)"/>
        public static void RestoreBinding(Type bindingType)
        {
            EnsureTypeIsNotBaseBinding(bindingType, nameof(bindingType));
            EnsureBindingRegistered(bindingType);

            _registeredBindings[bindingType].Restore();
        }

        /// <typeparam name="T">The binding of the specified type to restore.</typeparam>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="TypeArgumentException"/>
        /// <inheritdoc cref="RestoreBinding(Type)" path="/*[not(self::exception)]"/>
        public static void RestoreBinding<T>() where T : Binding
        {
            EnsureTypeIsNotBaseBinding<T>(nameof(T));
            EnsureBindingRegistered(typeof(T));

            _registeredBindings[typeof(T)].Restore();
        }

        #endregion

        #region Private Methods

        private static void ForceDeregisterBinding(Binding binding)
        {
            binding.Applied -= OnBindingApplied;
            binding.Restored -= OnBindingRestored;

            _registeredBindings.Remove(binding.GetType());
        }

        private static void ForceDeregisterBinding(Type bindingType)
        {
            ForceDeregisterBinding(_registeredBindings[bindingType]);
        }

        private static void OnBindingRegistered(Binding binding)
        {
            TB.Instance.LogDebug($"Registered binding '{binding.Name}'.");
            binding.Applied += OnBindingApplied;
            binding.Restored += OnBindingRestored;
            _registeredBindings.Add(binding.GetType(), binding);

            BindingRegistered?.Invoke(binding);
        }

        private static void OnBindingDeregistered(Binding binding)
        {
            TB.Instance.LogDebug($"Deregistered binding '{binding.Name}'.");
            binding.Applied -= OnBindingApplied;
            binding.Restored -= OnBindingRestored;
            _registeredBindings.Remove(binding.GetType());

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
            TB.Instance.LogDebug("Registering vanilla bindings...");
            RegisterBinding<NailBinding>();
            RegisterBinding<ShellBinding>();
            RegisterBinding<SoulBinding>();
            RegisterBinding<CharmsBinding>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="binding"></param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        private static void EnsureBindingRegistered(Binding binding)
        {
            if (binding is null)
                throw new ArgumentNullException(nameof(binding));

            EnsureBindingRegistered(binding.GetType());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bindingType"></param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        private static void EnsureBindingRegistered(Type bindingType)
        {
            if (bindingType is null)
                throw new ArgumentNullException(nameof(bindingType));

            if (!IsBindingRegistered(bindingType))
                throw new InvalidOperationException(ExcTypeNotRegistered);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <param name="paramName"></param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        private static void EnsureTypeIsNotBaseBinding(Type type, string paramName)
        {
            if (type is null)
                throw new ArgumentNullException(paramName);

            if (!type.IsAssignableTo(typeof(Binding)))
                throw new ArgumentException("The specified type is not a valid binding.");

            if (type == typeof(Binding))
                throw new ArgumentException(ExcTypeIsBaseBinding, paramName);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeParamName"></param>
        /// <exception cref="TypeArgumentException"/>
        private static void EnsureTypeIsNotBaseBinding<T>(string typeParamName)
        {
            if (typeof(T) == typeof(Binding))
                throw new TypeArgumentException(ExcTypeIsBaseBinding, typeParamName);
        }

        #region Serialization

        private static void OnSerializing()
        {
            TB.Instance.LogDebug(nameof(BindingManager) + " OnSerializing");
            _serializedBindings = _registeredBindings.Values.ToList();
        }

        private static void OnDeserialized()
        {
            TB.Instance.LogDebug(nameof(BindingManager) + " OnDeserialized");

            CoroutineController.Start(RegisterDeserializedBindings());
        }

        private static IEnumerator RegisterDeserializedBindings()
        {
            yield return new WaitWhile(() => HeroController.instance is null);

            foreach (var binding in _serializedBindings)
            {
                var bindingType = binding.GetType();

                if (IsBindingRegistered(bindingType))
                    ForceDeregisterBinding(bindingType);

                RegisterBinding(binding);

                if (binding.WasApplied)
                    ApplyBinding(bindingType);
            }
        }

        #endregion

        #endregion
    }
}