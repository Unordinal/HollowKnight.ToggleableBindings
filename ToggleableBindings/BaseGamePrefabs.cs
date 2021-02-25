#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TeamCherry;
using ToggleableBindings.Extensions;
using UnityEngine;
using Vasi;

namespace ToggleableBindings
{
    internal static class BaseGamePrefabs
    {
        [NotNull] public static FakePrefab? ChallengeDoorCanvas { get; private set; }

        [NotNull] public static FakePrefab? NailButton { get; private set; }

        [NotNull] public static FakePrefab? ShellButton { get; private set; }

        [NotNull] public static FakePrefab? SoulButton { get; private set; }

        [NotNull] public static FakePrefab? CharmsButton { get; private set; }

        [NotNull] public static FakePrefab? ShopMenu { get; private set; }

        [NotNull] public static FakePrefab? ArrowU { get; private set; }

        [NotNull] public static FakePrefab? ArrowD { get; private set; }

        [NotNull] public static FakePrefab? CharmEquipMsg { get; private set; }

        public static void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            if (preloadedObjects == null)
            {
                ToggleableBindings.Instance.LogWarn($"Couldn't initialize {nameof(BaseGamePrefabs)} as '{nameof(preloadedObjects)}' was null.");
                return;
            }

            InitializeGGAtriumPrefabs(preloadedObjects["GG_Atrium"]);
            InitializeRoomMapperPrefabs(preloadedObjects["Room_mapper"]);
            InitializeMiscPrefabs();
        }

        private static void InitializeGGAtriumPrefabs(Dictionary<string, GameObject> preloadedObjects)
        {
            var challengeDoor = preloadedObjects["GG_Challenge_Door"];
            var challengeUIFsm = challengeDoor.FindChild("Door/Unlocked Set/Inspect").LocateMyFSM("Challenge UI");
            var showUIAction = challengeUIFsm.GetAction<ShowBossDoorChallengeUI>("Open UI");
            var challengeCanvasPrefab = showUIAction.prefab.Value;

            GameObject buttonsPanel = challengeCanvasPrefab.FindChild("Panel/Buttons");
            GameObject nailButton = buttonsPanel.FindChild("NailButton");
            GameObject shellButton = buttonsPanel.FindChild("HeartButton");
            GameObject soulButton = buttonsPanel.FindChild("SoulButton");
            GameObject charmsButton = buttonsPanel.FindChild("CharmsButton");

            ChallengeDoorCanvas = new FakePrefab(challengeCanvasPrefab, "GG_Challenge_Door_Canvas", true);
            NailButton = new FakePrefab(nailButton, "NailButton", true);
            ShellButton = new FakePrefab(shellButton, "HeartButton", true);
            SoulButton = new FakePrefab(soulButton, "SoulButton", true);
            CharmsButton = new FakePrefab(charmsButton, "CharmsButton", true);
        }

        private static void InitializeRoomMapperPrefabs(Dictionary<string, GameObject> preloadedObjects)
        {
            var shopMenu = preloadedObjects["Shop Menu"];
            var arrowU = shopMenu.FindChild("Arrow U");
            var arrowD = shopMenu.FindChild("Arrow D");

            ShopMenu = new FakePrefab(shopMenu, "Shop Menu", true);
            ArrowU = new FakePrefab(arrowU, "Arrow U", true);
            ArrowD = new FakePrefab(arrowD, "Arrow D", true);
        }

        private static void InitializeMiscPrefabs()
        {
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            var charmEquipMsg = allObjects.First(go => go.name == "Charm Equip Msg");
            CharmEquipMsg = new FakePrefab(charmEquipMsg, nameof(CharmEquipMsg), true);

            Resources.UnloadUnusedAssets();
        }
    }
}