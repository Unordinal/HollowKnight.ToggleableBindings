#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Modding;
using Modding.Patches;
using Newtonsoft.Json;
using UnityEngine;
using UnoHKUtils.Extensions;

namespace UnoHKUtils
{
    public class QuickSettings
    {
        /// <summary>
        /// Called when this <see cref="QuickSettings"/> object finishes initializing.
        /// </summary>
        public event Action? Initialized;

        /// <summary>
        /// Called when the global settings are saved to file.
        /// </summary>
        public event Action? GlobalSettingsSaved;

        /// <summary>
        /// Called when the global settings are loaded from file.
        /// </summary>
        public event Action? GlobalSettingsLoaded;

        /// <summary>
        /// Called when save-specific settings are saved to file. The first parameter is the save slot ID.
        /// </summary>
        public event Action<int>? SaveSettingsSaved;

        /// <summary>
        /// Called when save-specific settings are loaded from file. The first parameter is the save slot ID.
        /// </summary>
        public event Action<int>? SaveSettingsLoaded;

        private const string SettingsGlobalFileName = "Settings.Global.json";
        private const string SettingsSaveFileName = "Settings.Save{0}.json";

        private static readonly string _baseDataPath = Application.persistentDataPath + '/';
        private static readonly Dictionary<string, string> _modSettingsPathMap = new();

        private static readonly JsonSerializerSettings _serializerSettings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Error,
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = ShouldSerializeContractResolver.Instance
        };

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
        protected int? CurrentSaveSlot { get; private set; }

        public QuickSettings()
        {
            Log("QuickSettings: Attempting initialization via parameterless ctor...");

            _owningAssembly = Assembly.GetCallingAssembly();
            var modType = _owningAssembly
                .GetTypes()
                .Where(t => typeof(Mod).IsAssignableFrom(t))
                .FirstOrDefault();

            Initialize(modType?.Name);
        }

        public QuickSettings(Mod mod) : this(mod.GetType()) { }

        public QuickSettings(Type modType)
        {
            Initialize(modType.Name);
        }

        /// <summary>
        /// Initializes the <see cref="QuickSettings"/> with the specified <paramref name="modName"/>. Should be called in the constructor.
        /// </summary>
        /// <param name="modName"></param>
        [MemberNotNull(nameof(ModName), nameof(_owningAssembly))]
        protected void Initialize(string? modName)
        {
            if (modName is null)
                throw new ArgumentNullException(nameof(modName), $"Couldn't find the name of the mod to use. Try using '{nameof(QuickSettings)}(Type)'.");

            ModName = modName;

            _owningAssembly ??= Assembly.GetCallingAssembly();
            UpdateDefinedSettings();
            LoadGlobalSettings();
            AddHooks();

            Initialized?.Invoke();
        }

        /// <summary>
        /// Unloads this <see cref="QuickSettings"/> object. This will save all available values to file before cleaning up.
        /// Once this object is unloaded, you should not attempt to use it again. Create a new <see cref="QuickSettings"/> object instead.
        /// </summary>
        public void Unload()
        {
            RemoveHooks();

            SaveGlobalSettings();
            if (CurrentSaveSlot is not null)
                SaveSaveSettings();
        }

        private void UpdateDefinedSettings()
        {
            _globalSettings.Clear();
            _saveSettings.Clear();
            foreach (var type in _owningAssembly!.GetTypes())
            {
                foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    var attr = member.GetCustomAttribute<QuickSettingAttribute>(false);
                    if (attr is null)
                        continue;

                    QuickSettingInfo settingInfo = new(member, attr.SettingName, attr.IsPerSave);

                    if (!settingInfo.IsPerSave)
                        _globalSettings.Add(settingInfo);
                    else
                        _saveSettings.Add(settingInfo);

                    LogDebug($"Registered setting '{settingInfo.SettingName}'.");
                }
            }
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
            if (saveSlotID is null && CurrentSaveSlot is null)
                return null;

            return SettingsDirectory + string.Format(SettingsSaveFileName, saveSlotID ?? CurrentSaveSlot);
        }

        /// <summary>
        /// Saves the global settings to file. Called automatically via <see cref="GameManagerHooks.BeforeOnApplicationQuit"/>.
        /// </summary>
        public void SaveGlobalSettings()
        {
            Log("Saving global settings...");

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
            Log("Loading global settings...");

            var filePath = GetGlobalSettingsPath();
            if (!File.Exists(filePath))
                return;

            try
            {
                LoadAndDeserialize(filePath, _globalSettings);
                GlobalSettingsLoaded?.Invoke();

                Log("Global settings loaded!");
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
        /// Saves the save settings to file. A save must be currently loaded. Called automatically via <see cref="GameManagerHooks.BeforeSaveGame"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public void SaveSaveSettings()
        {
            if (CurrentSaveSlot is null)
                throw new InvalidOperationException("Can't save save-specific settings when a save isn't loaded.");

            Log($"Saving settings for save slot {CurrentSaveSlot}...");

            EnsureSettingsDirectory();

            string filePath = GetSaveSettingsPath()!;

            try
            {
                SerializeAndSave(filePath, _saveSettings);

                SaveSettingsSaved?.Invoke(CurrentSaveSlot.GetValueOrDefault());
                Log("Save settings saved!");
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
            if (CurrentSaveSlot is null)
                throw new InvalidOperationException("Can't load save-specific settings when a save isn't loaded.");

            Log($"Loading settings for save slot {CurrentSaveSlot}...");

            var filePath = GetSaveSettingsPath()!;
            if (!File.Exists(filePath))
                return;

            try
            {
                LoadAndDeserialize(filePath, _saveSettings);
                SaveSettingsLoaded?.Invoke(CurrentSaveSlot.GetValueOrDefault());
                Log("Save settings loaded!");
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

        private void EnsureSettingsDirectory()
        {
            Directory.CreateDirectory(SettingsDirectory);
        }

        private void AddHooks()
        {
            GameManagerHooks.BeforeOnApplicationQuit += GameManagerHooks_BeforeOnApplicationQuit;
            GameManagerHooks.AfterSetState += GameManagerHooks_AfterSetState;
            GameManagerHooks.AfterLoadGame += GameManagerHooks_AfterLoadGame;
            GameManagerHooks.BeforeSaveGame += GameManagerHooks_BeforeSaveGame;
        }

        private void RemoveHooks()
        {
            GameManagerHooks.BeforeOnApplicationQuit -= GameManagerHooks_BeforeOnApplicationQuit;
            GameManagerHooks.AfterSetState -= GameManagerHooks_AfterSetState;
            GameManagerHooks.AfterLoadGame -= GameManagerHooks_AfterLoadGame;
            GameManagerHooks.BeforeSaveGame -= GameManagerHooks_BeforeSaveGame;
        }

        private void GameManagerHooks_BeforeOnApplicationQuit(GameManager gameManager)
        {
            SaveGlobalSettings();
        }

        private void GameManagerHooks_AfterSetState(GameManager gameManager, GlobalEnums.GameState newState)
        {
            if (newState == GlobalEnums.GameState.MAIN_MENU)
                CurrentSaveSlot = null;
        }

        private void GameManagerHooks_AfterLoadGame(GameManager gameManager, int saveSlot, Action<bool> callback)
        {
            CurrentSaveSlot = saveSlot;
            LoadSaveSettings();
        }

        private void GameManagerHooks_BeforeSaveGame(GameManager gameManager, int saveSlot, Action<bool> callback)
        {
            CurrentSaveSlot = saveSlot;
            SaveSaveSettings();
        }

        private void SerializeAndSave(string filePath, in List<QuickSettingInfo> settings)
        {
            var settingsData = new List<QuickSettingData>();
            foreach (var setting in settings)
            {
                var settingData = new QuickSettingData
                {
                    SettingName = setting.SettingKey,
                    SettingValue = setting.MemberInfo.GetMemberValue(null)
                };
                settingsData.Add(settingData);
            }

            string serialized = JsonConvert.SerializeObject(settingsData, _serializerSettings);
            File.WriteAllText(filePath, serialized);
        }

        private void LoadAndDeserialize(string filePath, in List<QuickSettingInfo> settings)
        {
            string serialized = File.ReadAllText(filePath);
            var settingsData = JsonConvert.DeserializeObject<List<QuickSettingData>>(serialized, _serializerSettings);

            var settingsDict = settingsData?.Where(d => d.SettingName is not null).ToDictionary(d => d.SettingName, d => d.SettingValue);
            if (settingsDict is not null)
            {
                foreach (var setting in settings)
                {
                    if (settingsDict.TryGetValue(setting.SettingKey, out var value))
                        setting.MemberInfo.SetMemberValue(null, value);
                }
            }
        }

        private void Log(object? message = null, LogLevel logLevel = LogLevel.Info)
        {
            string prefix = nameof(QuickSettings);
            if (_modName is not null)
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