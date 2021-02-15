#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ToggleableBindings.Collections;
using ToggleableBindings.Exceptions;
using ToggleableBindings.Extensions;
using ToggleableBindings.HKQuickSettings;
using ToggleableBindings.Utility;
using ToggleableBindings.VanillaBindings;
using UnityEngine;
using TB = ToggleableBindings.ToggleableBindings;

namespace ToggleableBindings
{
    /// <summary>
    /// Manages bindings and provides an API for applying and restoring them.
    /// </summary>
    public static class BindingManager
    {
        #region Exception Messages

        private const string ExcTypeIsBaseBinding = "The specified type must not be equal to 'typeof(" + nameof(Binding) + ")'.";
        private const string ExcTypeIsNotBinding = "The specified type does not inherit from '" + nameof(Binding) + "'.";
        private const string ExcNoBindingWithType = "No registered binding was of the specified type.";
        private const string ExcNoBindingWithName = "No registered binding had the specified name.";
        private const string ExcBindingAlreadyRegistered = "A binding with the same type is already registered.";

        #endregion

        private static readonly object _lock = new();
        private static readonly Dictionary<Type, Binding> _registeredBindings = new();
        private static readonly Dictionary<string, Type> _bindingNameTypeMap = new();
        private static readonly Type _abstractBindingType = typeof(Binding);

        [QuickSetting(true, nameof(RegisteredBindings))]
        private static ICollection<Binding> _serializedBindings = new List<Binding>();

        /// <summary>
        /// Invoked when a binding is successfully registered.
        /// </summary>
        public static event BindingEventHandler? BindingRegistered;
        /// <summary>
        /// Invoked when a binding is successfully deregistered.
        /// </summary>
        public static event BindingEventHandler? BindingDeregistered;
        /// <summary>
        /// Invoked when a registered binding is applied (enabled).
        /// </summary>
        public static event BindingEventHandler? BindingApplied;
        /// <summary>
        /// Invoked when a registered binding is restored (disabled).
        /// </summary>
        public static event BindingEventHandler? BindingRestored;

        /// <summary>
        /// Gets a read-only dictionary that provides a view of the currently registered binding types and their associated binding objects.
        /// </summary>
        public static IReadOnlyDictionary<Type, Binding> RegisteredBindings { get; } = new ReadOnlyDictionary<Type, Binding>(_registeredBindings);

        internal static void Initialize()
        {
            TB.MainMenuOrQuit += CleanUpForQuit;

            RegisterVanillaBindings();
        }

        internal static void Unload()
        {
            TB.MainMenuOrQuit -= CleanUpForQuit;

            RestoreAndClearBindings();
        }

        private static void RegisterVanillaBindings()
        {
            RegisterBinding<NailBinding>();
            RegisterBinding<ShellBinding>();
            RegisterBinding<SoulBinding>();
            RegisterBinding<CharmsBinding>();
        }

        private static void CleanUpForQuit()
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

                RestoreAndClearBindings();
            };
        }

        private static void RestoreAndClearBindings()
        {
            foreach (var binding in RegisteredBindings.Values)
            {
                binding.Restore();
                binding.Applied -= OnBindingApplied;
                binding.Restored -= OnBindingRestored;
            }

            lock (_lock)
                _registeredBindings.Clear();
        }

        #region IsBindingRegistered

        /// <summary>
        /// Checks whether the specified binding is registered.
        /// <para/>
        /// Note that this will check if the type of <paramref name="binding"/> is registered first
        /// and then check for reference equality between the
        /// specified binding and the registered binding.
        /// </summary>
        /// <param name="binding">The binding to check for.</param>
        /// <returns>
        /// <see langword="true"/> if the specified binding is registered; otherwise, <see langword="false"/>.
        /// Returns <see langword="false"/> if the type is registered but the registered binding isn't the
        /// same reference as <paramref name="binding"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"/>
        public static bool IsBindingRegistered(Binding binding)
        {
            if (binding is null)
                throw new ArgumentNullException(nameof(binding));

            lock (_lock)
                return RegisteredBindings.TryGetValue(binding.GetType(), out var registered) && binding == registered;
        }

        /// <summary>
        /// Checks whether a binding of the specified type is registered.
        /// </summary>
        /// <param name="bindingType">The type of the binding to check for.</param>
        /// <returns><see langword="true"/> if a binding of the specified type is registered; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public static bool IsBindingRegistered(Type bindingType)
        {
            TypeIsValidBinding(bindingType, nameof(bindingType)).ThrowIfUnsuccessful();

            lock (_lock)
                return RegisteredBindings.ContainsKey(bindingType);
        }

        /// <summary>
        /// Checks whether a binding with the specified name is registered.
        /// </summary>
        /// <param name="bindingName">The name of the binding to check for.</param>
        /// <returns><see langword="true"/> if a binding with the specified name is registered; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static bool IsBindingRegistered(string bindingName)
        {
            if (bindingName is null)
                throw new ArgumentNullException(nameof(bindingName));

            lock (_lock)
                return _bindingNameTypeMap.ContainsKey(bindingName);
        }

        /// <typeparam name="T"><inheritdoc cref="IsBindingRegistered(Type)" path="/param[1]"/></typeparam>
        /// <exception cref="TypeArgumentException"/>
        /// <inheritdoc cref="IsBindingRegistered(Type)" path="/*[not(self::exception)]"/>
        public static bool IsBindingRegistered<T>() where T : Binding
        {
            TypeIsValidBinding<T>(nameof(T)).ThrowIfUnsuccessful();

            lock (_lock)
                return RegisteredBindings.ContainsKey(typeof(T));
        }

        #endregion

        #region RegisterBinding

        /// <summary>
        /// Registers the specified binding. Only one binding of a given type
        /// can be registered at a time.
        /// </summary>
        /// <param name="binding">The binding to register.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        public static void RegisterBinding(Binding binding)
        {
            if (binding is null)
                throw new ArgumentNullException(nameof(binding));

            Type bindingType = binding.GetType();
            if (IsBindingRegistered(bindingType))
                throw new InvalidOperationException(ExcBindingAlreadyRegistered);

            binding.Applied += OnBindingApplied;
            binding.Restored += OnBindingRestored;

            lock (_lock)
            {
                _registeredBindings.Add(bindingType, binding);
                _bindingNameTypeMap.Add(binding.Name, bindingType);
            }

            OnBindingRegistered(binding);
        }

        /// <summary>
        /// Registers a new binding of the specified type. Only one binding of a given type can be registered at a time.
        /// </summary>
        /// <typeparam name="T">The type of binding to create and register.</typeparam>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="TypeArgumentException"/>
        public static void RegisterBinding<T>() where T : Binding, new()
        {
            TypeIsValidBinding<T>(nameof(T)).ThrowIfUnsuccessful();

            RegisterBinding(new T());
        }

        private static void OnBindingRegistered(Binding binding)
        {
            TB.Instance.LogDebug($"Registered binding '{binding.Name}'.");
            BindingRegistered?.Invoke(binding);
        }

        #endregion

        #region DeregisterBinding

        /// <summary>
        /// Deregisters the specified binding.
        /// </summary>
        /// <param name="binding">The binding to deregister.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        public static void DeregisterBinding(Binding binding)
        {
            if (binding is null)
                throw new ArgumentNullException(nameof(binding));

            Type bindingType = binding.GetType();
            if (!IsBindingRegistered(bindingType))
                throw new InvalidOperationException(ExcNoBindingWithType);

            binding.Applied -= OnBindingApplied;
            binding.Restored -= OnBindingRestored;

            lock (_lock)
            {
                _registeredBindings.Remove(bindingType);
                _bindingNameTypeMap.Remove(binding.Name);
            }

            OnBindingDeregistered(binding);
        }

        /// <summary>
        /// Deregisters the binding of the specified type.
        /// </summary>
        /// <param name="bindingType">The type of the binding to deregister.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        public static void DeregisterBinding(Type bindingType)
        {
            TypeIsValidBinding(bindingType, nameof(bindingType)).ThrowIfUnsuccessful();
            TypeIsRegisteredBinding(bindingType).ThrowIfUnsuccessful();

            Binding binding;
            lock (_lock)
                binding = RegisteredBindings[bindingType];

            DeregisterBinding(binding);
        }

        /// <summary>
        /// Deregisters the binding with the specified name.
        /// </summary>
        /// <param name="bindingName">The name of the binding to deregister.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        public static void DeregisterBinding(string bindingName)
        {
            if (bindingName is null)
                throw new ArgumentNullException(nameof(bindingName));

            if (!IsBindingRegistered(bindingName))
                throw new InvalidOperationException(ExcNoBindingWithName);

            Type bindingType;
            lock (_lock)
                bindingType = _bindingNameTypeMap[bindingName];

            DeregisterBinding(bindingType);
        }

        /// <typeparam name="T"><inheritdoc cref="DeregisterBinding(Type)" path="/param[1]"/></typeparam>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="TypeArgumentException"/>
        /// <inheritdoc cref="DeregisterBinding(Type)" path="/*[not(self::exception)]"/>
        public static void DeregisterBinding<T>() where T : Binding
        {
            TypeIsValidBinding<T>(nameof(T)).ThrowIfUnsuccessful();

            DeregisterBinding(typeof(T));
        }

        private static void OnBindingDeregistered(Binding binding)
        {
            TB.Instance.LogDebug($"Deregistered binding '{binding.Name}'.");
            BindingDeregistered?.Invoke(binding);
        }

        #endregion

        #region GetBinding

        /// <summary>
        /// Gets the registered binding of the specified type.
        /// </summary>
        /// <param name="bindingType">The type of the binding to get.</param>
        /// <returns>The registered binding of the specified type.</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        public static Binding GetBinding(Type bindingType)
        {
            TypeIsValidBinding(bindingType, nameof(bindingType)).ThrowIfUnsuccessful();

            return TypeIsRegisteredBinding(bindingType).GetValueOrThrow();
        }

        /// <summary>
        /// Gets the registered binding with the specified name.
        /// </summary>
        /// <param name="bindingName">The name of the binding to get.</param>
        /// <returns>The registered binding with the specified name.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        public static Binding GetBinding(string bindingName)
        {
            if (bindingName is null)
                throw new ArgumentNullException(nameof(bindingName));

            return NameIsRegisteredBinding(bindingName).GetValueOrThrow();
        }

        /// <typeparam name="T"><inheritdoc cref="GetBinding(Type)" path="/param[1]"/></typeparam>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="TypeArgumentException"/>
        /// <inheritdoc cref="GetBinding(Type)" path="/*[not(self::exception)]"/>
        public static T GetBinding<T>() where T : Binding
        {
            TypeIsValidBinding<T>(nameof(T)).ThrowIfUnsuccessful();

            return TypeIsRegisteredBinding<T>().GetValueOrThrow();
        }

        /// <summary>
        /// Attempts to get the registered binding of the specified type.
        /// </summary>
        /// <param name="bindingType"><inheritdoc cref="GetBinding(Type)"/></param>
        /// <param name="value">If successful, the matched binding; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if a binding of the specified type was found; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetBinding([NotNullWhen(true)] Type? bindingType, [NotNullWhen(true)] out Binding? value)
        {
            value = null;

            var validBinding = TypeIsValidBinding(bindingType, nameof(bindingType));
            if (!validBinding.IsSuccess)
                return false;

            var registeredBinding = TypeIsRegisteredBinding(bindingType);
            if (!registeredBinding.IsSuccess)
                return false;

            value = registeredBinding.Value;
            return true;
        }

        /// <summary>
        /// Attempts to get a registered binding with the specified name.
        /// </summary>
        /// <param name="bindingName"><inheritdoc cref="GetBinding(string)" path="/param[1]"/></param>
        /// <returns><see langword="true"/> if a binding with the specified name was found; otherwise, <see langword="false"/>.</returns>
        /// <inheritdoc cref="TryGetBinding(Type?, out Binding?)"/>
        public static bool TryGetBinding([NotNullWhen(true)] string? bindingName, [NotNullWhen(true)] out Binding? value)
        {
            value = null;

            if (bindingName is null)
                return false;

            var registeredBinding = NameIsRegisteredBinding(bindingName);
            if (!registeredBinding.IsSuccess)
                return false;

            value = registeredBinding.Value;
            return true;
        }

        /// <typeparam name="T"><inheritdoc cref="TryGetBinding(Type?, out Binding?)" path="/param[1]"/></typeparam>
        /// <inheritdoc cref="TryGetBinding(Type?, out Binding?)"/>
        public static bool TryGetBinding<T>([NotNullWhen(true)] out T? value) where T : Binding
        {
            value = default;

            var validBinding = TypeIsValidBinding<T>(nameof(T));
            if (!validBinding.IsSuccess)
                return false;

            var registeredBinding = TypeIsRegisteredBinding<T>();
            if (!registeredBinding.IsSuccess)
                return false;

            value = registeredBinding.Value;
            return true;
        }

        #endregion

        #region ApplyBinding

        /// <summary>
        /// Applies the registered binding of the specified type.
        /// </summary>
        /// <param name="bindingType">The type of the binding to apply.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        public static void ApplyBinding(Type bindingType)
        {
            Binding binding = GetBinding(bindingType);
            binding.Apply();
        }

        /// <summary>
        /// Applies the registered binding with the specified name.
        /// </summary>
        /// <param name="bindingName">The name of the binding to apply.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        public static void ApplyBinding(string bindingName)
        {
            Binding binding = GetBinding(bindingName);
            binding.Apply();
        }

        /// <typeparam name="T"></typeparam>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="TypeArgumentException"/>
        /// <inheritdoc cref="ApplyBinding(Type)" path="/*[not(self::exception)]"/>
        public static void ApplyBinding<T>() where T : Binding
        {
            Binding binding = GetBinding<T>();
            binding.Apply();
        }

        private static void OnBindingApplied(Binding binding)
        {
            TB.Instance.LogDebug($"Applied binding '{binding.Name}'.");
            BindingApplied?.Invoke(binding);
        }

        #endregion

        #region RestoreBinding

        /// <summary>
        /// Restores the registered binding of the specified type.
        /// </summary>
        /// <param name="bindingType">The type of the binding to restore.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        public static void RestoreBinding(Type bindingType)
        {
            Binding binding = GetBinding(bindingType);
            binding.Restore();
        }

        /// <summary>
        /// Restores the registered binding with the specified name.
        /// </summary>
        /// <param name="bindingName">The name of the binding to restore.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        public static void RestoreBinding(string bindingName)
        {
            Binding binding = GetBinding(bindingName);
            binding.Restore();
        }

        /// <typeparam name="T"><inheritdoc cref="RestoreBinding(Type)" path="/param[1]"/></typeparam>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="TypeArgumentException"/>
        /// <inheritdoc cref="RestoreBinding(Type)" path="/*[not(self::exception)]"/>
        public static void RestoreBinding<T>() where T : Binding
        {
            Binding binding = GetBinding<T>();
            binding.Restore();
        }

        private static void OnBindingRestored(Binding binding)
        {
            TB.Instance.LogDebug($"Restored binding '{binding.Name}'.");
            BindingRestored?.Invoke(binding);
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Checks that the passed type is not null,
        /// is not equal to 'typeof(<see cref="Binding"/>)',
        /// and inherits from <see cref="Binding"/>.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="paramName">The name of the parameter to show for argument exceptions.</param>
        /// <returns>
        /// <see cref="TryResult.Success"/> if successful;
        /// otherwise, a <see cref="TryResult"/> containing the exception that would have been thrown.
        /// </returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        private static TryResult TypeIsValidBinding([NotNull] Type? type, string paramName)
        {
            if (type is null)
#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.
                return new ArgumentNullException(paramName);
#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.

            if (type == _abstractBindingType)
                return new ArgumentException(ExcTypeIsBaseBinding, paramName);

            if (!type.IsAssignableTo(_abstractBindingType))
                return new ArgumentException(ExcTypeIsNotBinding);

            return TryResult.Success;
        }

        /// <typeparam name="T"></typeparam>
        /// <exception cref="TypeArgumentException"/>
        /// <inheritdoc cref="TypeIsValidBinding(Type?, string)" path="/*[not(self::exception)]"/>
        private static TryResult TypeIsValidBinding<T>(string typeParamName)
        {
            if (typeof(T) == _abstractBindingType)
                return new TypeArgumentException(ExcTypeIsBaseBinding, typeParamName);

            return TryResult.Success;
        }

        /// <summary>
        /// Checks that the passed type is in the dictionary of registered bindings.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><inheritdoc cref="TypeIsValidBinding(Type?, string)"/></returns>
        /// <exception cref="InvalidOperationException"/>
        private static TryResult<Binding> TypeIsRegisteredBinding(Type type)
        {
            lock (_lock)
            {
                if (!RegisteredBindings.TryGetValue(type, out Binding? value))
                    return new InvalidOperationException(ExcNoBindingWithType);

                return value;
            }
        }

        /// <summary>
        /// Checks that the passed name is in the name-to-type map.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns><inheritdoc cref="TypeIsRegisteredBinding(Type)"/></returns>
        /// <exception cref="InvalidOperationException"/>
        private static TryResult<Binding> NameIsRegisteredBinding(string name)
        {
            lock (_lock)
            {
                if (!_bindingNameTypeMap.TryGetValue(name, out Type? value))
                    return new InvalidOperationException(ExcNoBindingWithName);

                return TypeIsRegisteredBinding(value);
            }
        }

        /// <typeparam name="T"><inheritdoc cref="TypeIsRegisteredBinding(Type)" path="/param[1]"/></typeparam>
        /// <inheritdoc cref="TypeIsRegisteredBinding(Type)"/>
        private static TryResult<T> TypeIsRegisteredBinding<T>() where T : Binding
        {
            lock (_lock)
            {
                if (!RegisteredBindings.TryGetValue(typeof(T), out Binding? value))
                    return new InvalidOperationException(ExcNoBindingWithType);

                return (T)value;
            }
        }

        #endregion

        #region Serialization

        private static void OnSerializing()
        {
            TB.Instance.LogDebug(nameof(BindingManager) + ": OnSerializing");
            _serializedBindings = _registeredBindings.Values;
        }

        private static void OnDeserialized()
        {
            TB.Instance.LogDebug(nameof(BindingManager) + ": OnDeserialized");
            CoroutineController.Start(RegisterDeserializedBindings());
        }

        private static IEnumerator RegisterDeserializedBindings()
        {
            yield return new WaitWhile(() => HeroController.instance is null);

            foreach (var binding in _serializedBindings)
            {
                Type bindingType = binding.GetType();
                if (IsBindingRegistered(bindingType))
                {
                    binding.Applied -= OnBindingApplied;
                    binding.Restored -= OnBindingRestored;
                    lock (_lock)
                        _registeredBindings.Remove(bindingType);
                }

                RegisterBinding(binding);

                if (binding.WasApplied)
                    ApplyBinding(bindingType);
            }
        }

        #endregion
    }
}