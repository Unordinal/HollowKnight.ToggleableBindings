#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ToggleableBindings.Extensions;
using UnityEngine;
using Vasi;
using TB = ToggleableBindings.ToggleableBindings;
using TBUI = ToggleableBindings.UI;
using UObject = UnityEngine.Object;

namespace ToggleableBindings
{
    internal static class Prefabs
    {
        private static GameObject? _holderGO;
        private static GameObject? _ddolHolderGO;

        [NotNull] private static GameObject? _vanillaChallengeUI;
        private static GameObject? _vanillaNailButton;
        private static GameObject? _vanillaShellButton;
        private static GameObject? _vanillaSoulButton;
        private static GameObject? _vanillaCharmsButton;
        private static GameObject? _bindingsUI;
        private static GameObject? _bindingsUIBindingButton;

        internal static GameObject HolderGO => _holderGO ??= new GameObject(nameof(ToggleableBindings) + "::" + "Holder");

        internal static GameObject DDOLHolderGO
        {
            get
            {
                if (_ddolHolderGO is null)
                {
                    _ddolHolderGO = new GameObject(nameof(ToggleableBindings) + "::" + "DDOL Holder");
                    UObject.DontDestroyOnLoad(_ddolHolderGO);
                }

                return _ddolHolderGO;
            }
        }

        internal static GameObject VanillaChallengeUI => _vanillaChallengeUI;

        internal static GameObject BindingsUI => _bindingsUI ??= NewPrefab(TBUI.BindingsUI.GetPrefab());

        internal static GameObject BindingsUIBindingButton => _bindingsUIBindingButton ??= NewPrefab(TBUI.BindingsUIBindingButton.GetPrefab());

        public static GameObject VanillaNailButton => _vanillaNailButton ??= NewPrefabViaCanvasPath("Panel/Buttons/NailButton");

        public static GameObject VanillaShellButton => _vanillaShellButton ??= NewPrefabViaCanvasPath("Panel/Buttons/HeartButton");

        public static GameObject VanillaSoulButton => _vanillaSoulButton ??= NewPrefabViaCanvasPath("Panel/Buttons/SoulButton");

        public static GameObject VanillaCharmsButton => _vanillaCharmsButton ??= NewPrefabViaCanvasPath("Panel/Buttons/CharmsButton");

        public static void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            var challengeDoor = preloadedObjects["GG_Atrium"]["GG_Challenge_Door"];
            var challengeUIFsm = challengeDoor.FindChildByPath("Door/Unlocked Set/Inspect").LocateMyFSM("Challenge UI");
            var showUIAction = challengeUIFsm.GetAction<ShowBossDoorChallengeUI>("Open UI");
            var challengeCanvasPrefab = showUIAction.prefab.Value;

            _vanillaChallengeUI = NewPrefab(challengeCanvasPrefab, nameof(VanillaChallengeUI));
        }

        public static GameObject Instantiate(GameObject prefab, InstanceFlags instanceFlags = InstanceFlags.Default)
        {
            return Instantiate(prefab, null, instanceFlags);
        }

        public static GameObject Instantiate(GameObject prefab, GameObject? parent, InstanceFlags instanceFlags = InstanceFlags.Default)
        {
            if (prefab is null)
                throw new ArgumentNullException(nameof(prefab));

            bool startInactive = (instanceFlags & InstanceFlags.StartInactive) != 0;
            bool dontDestroyOnLoad = (instanceFlags & InstanceFlags.DontDestroyOnLoad) != 0;
            bool worldPositionDoesNotStay = (instanceFlags & InstanceFlags.WorldPositionDoesNotStay) != 0;

            var instance = UObject.Instantiate(prefab);
            instance.SetActive(!startInactive);
            if (dontDestroyOnLoad)
                UObject.DontDestroyOnLoad(instance);

            var actualParent = parent ?? (dontDestroyOnLoad ? DDOLHolderGO : HolderGO);
            instance.SetParent(actualParent, !worldPositionDoesNotStay);

            return instance;
        }

        private static GameObject NewPrefabViaCanvasPath(string path, [CallerMemberName] string? name = null)
        {
            return NewPrefab(VanillaChallengeUI.FindChildByPath(path), name);
        }

        private static GameObject NewPrefab(GameObject toPrefab, [CallerMemberName] string? name = null)
        {
            TB.Instance.LogDebug($"Creating new prefab of object '{toPrefab.name}', with name '{name ?? toPrefab.name}'.");

            var prefab = Instantiate(toPrefab, InstanceFlags.StartInactive | InstanceFlags.DontDestroyOnLoad);
            prefab.name = GetPrefabName(name ?? prefab.name);

            return prefab;
        }

        private static string GetPrefabName(string name)
        {
            return $"{nameof(ToggleableBindings)}::{nameof(Prefabs)}.{name}"; // ToggleableBindings::Prefabs.MyPrefabName
        }
    }
}