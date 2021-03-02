#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using GlobalEnums;
using Modding;
using Modding.Patches;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ToggleableBindings.Extensions;
using ToggleableBindings.JsonNet;
using ToggleableBindings.Utility;
using UnityEngine;

namespace ToggleableBindings.HKQuickSettings
{
    internal class QuickSettings
    {
        /// <summary>
        /// Called when this <see cref="QuickSettings"/> object finishes initializing.
        /// </summary>
        public event Action? Initialized;

        /// <summary>
        /// Called when this <see cref="QuickSettings"/> object finishes unloading.
        /// </summary>
        public event Action? Unloaded;

        /// <summary>
        /// Called when the global settings are saved to file.
        /// </summary>
        public event Action? GlobalSettingsSaved;

        /// <summary>
        /// Called when the global settings are loaded from file.
        /// </summary>
        public event Action? GlobalSettingsLoaded;

        /// <summary>
        /// Called when save-specific settings are saved to file.
        /// When invoked via <see cref="On.GameManager.SaveGame"/>,
        /// this is right before the actual game save is saved.
        /// <para/>
        /// The first parameter is the save slot ID.
        /// </summary>
        public event Action<int>? SaveSettingsSaved;

        /// <summary>
        /// Called when save-specific settings are loaded from file.
        /// When invoked via <see cref="On.GameManager.LoadGame"/>,
        /// this is right after the actual game save is loaded.
        /// <para/>
        /// The first parameter is the save slot ID.
        /// </summary>
        public event Action<int>? SaveSettingsLoaded;

        private const string SettingsGlobalFileName = "Settings.Global.json";
        private const string SettingsSaveFileName = "Settings.Save{0}.json";

        private static readonly string _baseDataPath = Application.persistentDataPath + '/';
        private static readonly Dictionary<string, string> _modSettingsPathMap = new();

        private static readonly JsonSerializerSettings _serializerSettings = new()
        {
            Formatting = Formatting.Indented,
            ObjectCreationHandling = ObjectCreationHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Populate,
            ReferenceLoopHandling = ReferenceLoopHandling.Error,
            TypeNameHandling = TypeNameHandling.Auto,
            ConstructorHandling = ConstructorHandling.Default,
            ContractResolver = ShouldSerializeContractResolver.Instance,
            Binder = JsonSerializationBinder.Instance,
            Error = LogAndIgnoreSerializationBindingErrors
        };

        private static JsonSerializer Serializer { get; } = JsonSerializer.Create(_serializerSettings);

        private Assembly? _owningAssembly;
        private string? _modName;
        private readonly List<QuickSettingInfo> _globalSettings = new();
        private readonly List<QuickSettingInfo> _saveSettings = new();

        private string SettingsDirectory => _baseDataPath + ModName + "." + nameof(QuickSettings) + '/';

        /// <summary>
        /// Gets the name of the mod that initialized this settings object.
        /// </summary>
        protected string ModName
        {
            get => _modName ?? throw new InvalidOperationException($"This {nameof(QuickSettings)} object wasn't initialized properly.");
            private set => _modName = value;
        }

        /// <summary>
        /// Gets the current save slot that's loaded, or <see langword="null"/> if there isn't one.
        /// </summary>
        public int? CurrentSaveSlot { get; internal set; }

        /// <summary>
        /// Creates a new <see cref="QuickSettings"/> instance. In the case of this constructor failing,
        /// you should try using <see cref="QuickSettings(Mod)"/> or <see cref="QuickSettings(Type)"/>.
        /// </summary>
        public QuickSettings()
        {
            LogDebug("Attempting initialization via parameterless ctor...");

            _owningAssembly = Assembly.GetCallingAssembly();
            var modType = _owningAssembly
                .GetTypes()
                .Where(t => typeof(Mod).IsAssignableFrom(t))
                .FirstOrDefault();

            Initialize(modType?.Name);
        }

        /// <summary>
        /// Creates a new <see cref="QuickSettings"/> instance from the
        /// specified <see cref="Mod"/> instance.
        /// </summary>
        /// <param name="mod"></param>
        public QuickSettings(Mod mod) : this(mod.GetType()) { }

        /// <summary>
        /// Creates a new <see cref="QuickSettings"/> instance from the
        /// specified <see cref="Type"/>.
        /// </summary>
        /// <param name="modType"></param>
        public QuickSettings(Type modType)
        {
            if (!modType.IsAssignableTo(typeof(Mod)))
                throw new ArgumentException("Cannot initialize a QuickSettings instance with a type that does not derive from Mod.");

            Initialize(modType.Name);
        }

        /// <summary>
        /// Initializes the <see cref="QuickSettings"/> with the specified <paramref name="modName"/>. Should be called in the constructor.
        /// </summary>
        /// <param name="modName"></param>
        [MemberNotNull(nameof(ModName), nameof(_owningAssembly))]
        protected void Initialize(string? modName)
        {
            if (modName == null)
                throw new ArgumentNullException(nameof(modName), $"Couldn't find the name of the mod to use. Try using '{nameof(QuickSettings)}(Type)'.");

            ModName = modName;

            _owningAssembly ??= Assembly.GetCallingAssembly();
            RegisterDefinedSettings();
            LoadGlobalSettings();
            AddHooks();

            if (GameManager.instance && GameManager.instance.gameState is GameState.PLAYING or GameState.PAUSED)
            {
                CurrentSaveSlot = GameManager.instance.profileID;
                LoadSaveSettings();
            }

            Initialized?.Invoke();
        }

        /// <summary>
        /// Unloads this <see cref="QuickSettings"/> object. This will save all available values to file before cleaning up.
        /// Once this object is unloaded, you should not attempt to use it again. Create a new <see cref="QuickSettings"/> object instead.
        /// </summary>
        public void Unload()
        {
            RemoveHooks();
            SaveAllSettings();
            Unloaded?.Invoke();
        }

        /// <summary>
        /// Adds a setting. Functionally equivalent to declaring <see cref="QuickSettingAttribute"/> on a member; you may wish to use that instead.
        /// <para id="doesNotAutoSave">
        /// Note: This does not cause a save to occur; if you wish to ensure the new state is saved to file,
        /// call <see cref="SaveAllSettings"/> or one of the other save methods.
        /// </para>
        /// </summary>
        /// <param name="member">The member to add as a setting.</param>
        /// <param name="settingName">The optional name of the setting. If <see langword="null"/>, uses <see cref="MemberInfo.Name"/> instead.</param>
        /// <param name="isPerSave">If <see langword="true"/>, this setting is save slot-specific.</param>
        public void AddSetting(MemberInfo member, string? settingName = null, bool isPerSave = false)
        {
            QuickSettingInfo settingInfo = new(member, settingName, isPerSave);
            var settings = GetSettingsList(isPerSave);
            settings.Add(settingInfo);
        }

        /// <summary>
        /// Removes the setting with the specified name.
        /// <inheritdoc cref="AddSetting(MemberInfo, string?, bool)" path="//para[@id='doesNotAutoSave']"/>
        /// </summary>
        /// <param name="settingName">The name of the setting to remove.</param>
        /// <inheritdoc cref="RemoveSetting(Func{QuickSettingInfo, bool}, bool)"/>
        public bool RemoveSetting(string settingName, bool isPerSave)
        {
            return RemoveSetting(si => si.Name == settingName, isPerSave);
        }

        /// <summary>
        /// Removes the setting with the specified <see cref="MemberInfo"/>.
        /// <inheritdoc cref="AddSetting(MemberInfo, string?, bool)" path="//para[@id='doesNotAutoSave']"/>
        /// </summary>
        /// <param name="member">The member to remove as a setting.</param>
        /// <inheritdoc cref="RemoveSetting(Func{QuickSettingInfo, bool}, bool)"/>
        public bool RemoveSetting(MemberInfo member, bool isPerSave)
        {
            return RemoveSetting(si => si.MemberInfo == member, isPerSave);
        }

        /// <summary>
        /// Removes the first setting that matches the specified <paramref name="predicate"/>.
        /// <inheritdoc cref="AddSetting(MemberInfo, string?, bool)" path="//para[@id='doesNotAutoSave']"/>
        /// </summary>
        /// <param name="predicate">The predicate to match.</param>
        /// <param name="isPerSave">If <see langword="true"/>, the setting to remove is save slot-specific.</param>
        /// <returns><see langword="true"/> if the setting was successfully found and removed; otherwise, <see langword="false"/>.</returns>
        public bool RemoveSetting(Func<QuickSettingInfo, bool> predicate, bool isPerSave)
        {
            var settings = GetSettingsList(isPerSave);
            int removeAt = settings.FindIndex(si => predicate(si));
            if (removeAt is -1)
                return false;

            settings.RemoveAt(removeAt);
            return true;
        }

        private void RegisterDefinedSettings()
        {
            const BindingFlags memberFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

            _globalSettings.Clear();
            _saveSettings.Clear();
            foreach (var type in _owningAssembly!.GetTypes())
            {
                foreach (var member in type.GetMembers(memberFlags))
                {
                    var attr = member.GetCustomAttribute<QuickSettingAttribute>(false);
                    if (attr == null)
                        continue;

                    AddSetting(member, attr.SettingName, attr.IsPerSave);

                    LogDebug($"Registered setting '{attr.SettingName}'.");
                }
            }
        }

        private List<QuickSettingInfo> GetSettingsList(bool isPerSave)
        {
            return !isPerSave ? _globalSettings : _saveSettings;
        }

        /// <summary>
        /// Gets the global settings file path. The file is not guaranteed to exist.
        /// </summary>
        /// <returns>The path to the global settings file for this mod.</returns>
        public string GetGlobalSettingsPath()
        {
            return SettingsDirectory + SettingsGlobalFileName;
        }

        /// <summary>
        /// Gets the save settings file path for the specified <paramref name="saveSlotID"/>, or the currently loaded save
        /// slot if <paramref name="saveSlotID"/> is <see langword="null"/> and one is currently loaded.
        /// The file is not guaranteed to exist.
        /// </summary>
        /// <param name="saveSlotID">The save slot ID to use.</param>
        /// <returns>
        /// The path to the save settings file of the specified slot for this mod, or <see langword="null"/>
        /// if <paramref name="saveSlotID"/> is <see langword="null"/> and no save is currently loaded. The path always
        /// separates directories using '<c>/</c>'.
        /// </returns>
        public string? GetSaveSettingsPath(int? saveSlotID = null)
        {
            if (saveSlotID == null && CurrentSaveSlot == null)
                return null;

            return SettingsDirectory + string.Format(SettingsSaveFileName, saveSlotID ?? CurrentSaveSlot);
        }

        /// <summary>
        /// Saves both global and save settings to file. Save settings will only be saved if a save is currently loaded.
        /// </summary>
        public void SaveAllSettings()
        {
            SaveGlobalSettings();
            if (CurrentSaveSlot != null)
                SaveSaveSettings();
        }

        /// <summary>
        /// Saves the global settings to file. Called automatically via <see cref="On.GameManager.OnApplicationQuit"/>.
        /// </summary>
        public void SaveGlobalSettings()
        {
            LogDebug("Saving global settings...");

            EnsureSettingsDirectory();
            var filePath = GetGlobalSettingsPath();

            try
            {
                SerializeAndSave(filePath, _globalSettings);
                GlobalSettingsSaved?.Invoke();

                LogDebug("Global settings saved!");
            }
            catch (Exception) when (LogExc("Failed to save settings."))
            { }
            catch (IOException ex)
            {
                LogError("There was a problem writing the settings file: " + ex.Message);
                LogDebug(ex);
            }
            catch (JsonSerializationException ex)
            {
                LogError("Failed to serialize the data to the settings file: " + ex.Message);
                LogDebug(ex);
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                LogDebug(ex);
            }
        }

        private void LoadGlobalSettings()
        {
            LogDebug("Loading global settings...");

            var filePath = GetGlobalSettingsPath();
            if (!File.Exists(filePath))
                return;

            try
            {
                LoadAndDeserialize(filePath, _globalSettings);
                GlobalSettingsLoaded?.Invoke();

                LogDebug("Global settings loaded!");
            }
            catch (Exception) when (LogExc("Failed to load settings."))
            { }
            catch (FileNotFoundException ex)
            {
                LogError("Tried to read from a settings file that doesn't exist.");
                LogDebug(ex);
            }
            catch (IOException ex)
            {
                LogError("There was a problem reading the settings file: " + ex.Message);
                LogDebug(ex);
            }
            catch (JsonSerializationException ex)
            {
                LogError("Failed to deserialize the data within the settings file: " + ex.Message);
                LogDebug(ex);
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                LogDebug(ex);
            }
        }

        /// <summary>
        /// Saves the save settings to file. A save must be currently loaded. 
        /// Called automatically via <see cref="On.GameManager.SaveGame"/> and happens before
        /// the actual game save is saved.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public void SaveSaveSettings()
        {
            if (CurrentSaveSlot == null)
                throw new InvalidOperationException("Can't save save-specific settings when a save isn't loaded.");

            LogDebug($"Saving settings for save slot {CurrentSaveSlot}...");

            EnsureSettingsDirectory();

            string filePath = GetSaveSettingsPath()!;

            try
            {
                SerializeAndSave(filePath, _saveSettings);

                SaveSettingsSaved?.Invoke(CurrentSaveSlot.GetValueOrDefault());
                LogDebug("Save settings saved!");
            }
            catch (Exception) when (LogExc("Failed to save settings."))
            { }
            catch (IOException ex)
            {
                LogError("There was a problem writing the settings file: " + ex.Message);
                LogDebug(ex);
            }
            catch (JsonSerializationException ex)
            {
                LogError("Failed to serialize the data to the settings file: " + ex.Message);
                LogDebug(ex);
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                LogDebug(ex);
            }
        }

        private void LoadSaveSettings()
        {
            if (CurrentSaveSlot == null)
                throw new InvalidOperationException("Can't load save-specific settings when a save isn't loaded.");

            LogDebug($"Loading settings for save slot {CurrentSaveSlot}...");

            var filePath = GetSaveSettingsPath()!;
            if (!File.Exists(filePath))
                return;

            try
            {
                LoadAndDeserialize(filePath, _saveSettings);
                SaveSettingsLoaded?.Invoke(CurrentSaveSlot.GetValueOrDefault());
                LogDebug("Save settings loaded!");
            }
            catch (Exception) when (LogExc("Failed to load settings."))
            { }
            catch (FileNotFoundException ex)
            {
                LogError("Tried to read from a settings file that doesn't exist: " + filePath);
                LogDebug(ex);
            }
            catch (IOException ex)
            {
                LogError("There was a problem reading the settings file: " + ex.Message);
                LogDebug(ex);
            }
            catch (JsonSerializationException ex)
            {
                LogError("Failed to deserialize the data within the settings file: " + ex.Message);
                LogDebug(ex);
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                LogDebug(ex);
            }
        }

        private void EnsureSettingsDirectory()
        {
            Directory.CreateDirectory(SettingsDirectory);
        }

        private static void LogAndIgnoreSerializationBindingErrors(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
        {
            /*if (args.CurrentObject == args.ErrorContext.OriginalObject
            && InnerExceptionsAndSelf(args.ErrorContext.Error).OfType<JsonSerializationBinderException>().Any()
            && args.ErrorContext.OriginalObject.GetType().GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>)))*/
            if (args.CurrentObject == args.ErrorContext.OriginalObject && AnyBinderExceptions(args.ErrorContext.Error))
            {
                ToggleableBindings.Instance.LogError(args.ErrorContext.Error.Message);
                ToggleableBindings.Instance.Log("Ignore the above error if you recently uninstalled or disabled a mod.");
                args.ErrorContext.Handled = true;
            }

            static bool AnyBinderExceptions(Exception ex)
            {
                return InnerExceptionsAndSelf(ex).Any(e => e is JsonSerializationBinderException);
            }

            static IEnumerable<Exception> InnerExceptionsAndSelf(Exception ex)
            {
                while (ex != null)
                {
                    yield return ex;
                    ex = ex.InnerException;
                }
            }
        }

        private void AddHooks()
        {
            On.GameManager.OnApplicationQuit += GameManager_OnApplicationQuit;
            On.GameManager.SetState += GameManager_SetState;
            On.GameManager.LoadGame += GameManager_LoadGame;
            On.GameManager.SaveGame_int_Action1 += GameManager_SaveGame;
        }

        private void RemoveHooks()
        {
            On.GameManager.OnApplicationQuit -= GameManager_OnApplicationQuit;
            On.GameManager.SetState -= GameManager_SetState;
            On.GameManager.LoadGame -= GameManager_LoadGame;
            On.GameManager.SaveGame_int_Action1 -= GameManager_SaveGame;
        }

        private void GameManager_OnApplicationQuit(On.GameManager.orig_OnApplicationQuit orig, GameManager self)
        {
            SaveGlobalSettings();
            orig(self);
        }

        private void GameManager_SetState(On.GameManager.orig_SetState orig, GameManager self, GameState newState)
        {
            orig(self, newState);
            if (newState == GlobalEnums.GameState.MAIN_MENU)
                CurrentSaveSlot = null;
        }

        private void GameManager_LoadGame(On.GameManager.orig_LoadGame orig, GameManager self, int saveSlot, Action<bool> callback)
        {
            orig(self, saveSlot, callback);
            CurrentSaveSlot = saveSlot;

            CoroutineBuilder.New
                .WithYield(new WaitWhile(() => !HeroController.instance), null)
                .WithAction(LoadSaveSettings)
                .Start();
        }

        private void GameManager_SaveGame(On.GameManager.orig_SaveGame_int_Action1 orig, GameManager self, int saveSlot, Action<bool> callback)
        {
            CurrentSaveSlot = saveSlot;
            SaveSaveSettings();
            orig(self, saveSlot, callback);
        }

        private void SerializeAndSave(string filePath, in List<QuickSettingInfo> settings)
        {
            var settingsData = new List<QuickSettingData>();
            foreach (var setting in settings)
            {
                const BindingFlags methodFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                var declaringType = setting.MemberInfo.DeclaringType;

                var onSerializing = declaringType.GetMethod("OnSerializing", methodFlags);
                onSerializing?.Invoke(null, null);

                var settingData = new QuickSettingData
                {
                    SettingName = setting.Key,
                    SettingValue = setting.MemberInfo.GetMemberValue(null)
                };
                settingsData.Add(settingData);

                var onSerialized = declaringType.GetMethod("OnSerialized", methodFlags);
                onSerialized?.Invoke(null, null);
            }

            string serialized = JsonConvert.SerializeObject(settingsData, _serializerSettings);
            File.WriteAllText(filePath, serialized);
        }

        private void LoadAndDeserialize(string filePath, in List<QuickSettingInfo> settings)
        {
            const BindingFlags methodFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            string json = File.ReadAllText(filePath);
            var jArray = JArray.Parse(json);

            var settingsDict = settings.ToDictionary(s => s.Key);
            foreach (var setting in jArray.Children<JObject>())
            {
                var settingName = setting.Value<string>(nameof(QuickSettingData.SettingName));
                if (settingsDict.TryGetValue(settingName, out var settingInfo))
                {
                    var declaringType = settingInfo.MemberInfo.DeclaringType;

                    var onDeserializing = declaringType.GetMethod("OnDeserializing", methodFlags);
                    onDeserializing?.Invoke(null, null);

                    Type memberType = settingInfo.MemberInfo.GetUnderlyingType();
                    var valueToken = setting.GetValue(nameof(QuickSettingData.SettingValue));
                    var memberValue = valueToken.ToObject(memberType, Serializer);
                    settingInfo.MemberInfo.SetMemberValue(null, memberValue);

                    var onDeserialized = declaringType.GetMethod("OnDeserialized", methodFlags);
                    onDeserialized?.Invoke(null, null);
                }
                else
                    ToggleableBindings.Instance.LogError("Couldn't find a setting with the specified key: " + settingName);
            }
        }

        private void Log(object? message = null, LogLevel logLevel = LogLevel.Info)
        {
            string prefix = nameof(QuickSettings);
            if (_modName != null)
                prefix += $" ({_modName})";

            string full = $"[{prefix}] - {message}";
            Modding.Logger.Log(full, logLevel);
        }

        private void LogDebug(object? message) => Log(message, LogLevel.Debug);

        private void LogWarn(object? message) => Log(message, LogLevel.Warn);

        private void LogError(object? message) => Log(message, LogLevel.Error);

        private bool LogExc(object? message)
        {
            Log(message, LogLevel.Error);
            return false;
        }
    }
}