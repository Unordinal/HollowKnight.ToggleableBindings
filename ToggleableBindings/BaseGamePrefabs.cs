#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using ToggleableBindings.Extensions;
using ToggleableBindings.Utility;
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

        public static void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            var challengeDoor = preloadedObjects["GG_Atrium"]["GG_Challenge_Door"];
            var challengeUIFsm = challengeDoor.FindChild("Door/Unlocked Set/Inspect").LocateMyFSM("Challenge UI");
            var showUIAction = challengeUIFsm.GetAction<ShowBossDoorChallengeUI>("Open UI");
            var challengeCanvasPrefab = showUIAction.prefab.Value;

            GameObject buttonsPanel = challengeCanvasPrefab.FindChild("Panel/Buttons");
            GameObject nailButton = buttonsPanel.FindChild("NailButton");
            GameObject shellButton = buttonsPanel.FindChild("HeartButton");
            GameObject soulButton = buttonsPanel.FindChild("SoulButton");
            GameObject charmsButton = buttonsPanel.FindChild("CharmsButton");

            ChallengeDoorCanvas = new FakePrefab(challengeCanvasPrefab, "GG_Challenge_Door_Canvas");
            NailButton = new FakePrefab(nailButton, "NailButton");
            ShellButton = new FakePrefab(shellButton, "HeartButton");
            SoulButton = new FakePrefab(soulButton, "SoulButton");
            CharmsButton = new FakePrefab(charmsButton, "CharmsButton");
        }
    }
}