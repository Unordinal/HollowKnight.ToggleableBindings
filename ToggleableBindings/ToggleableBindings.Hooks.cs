#nullable enable

using System;
using System.Collections;
using UnityEngine;

namespace ToggleableBindings
{
    public sealed partial class ToggleableBindings
    {
        internal static event Action? MainMenuOrQuit;

        private void AddHooks()
        {
            On.GameManager.Update += GameManager_Update;
            On.GameManager.OnApplicationQuit += GameManager_OnApplicationQuit;
            On.GameManager.ReturnToMainMenu += GameManager_ReturnToMainMenu;
        }

        private void RemoveHooks()
        {
            On.GameManager.Update -= GameManager_Update;
            On.GameManager.OnApplicationQuit -= GameManager_OnApplicationQuit;
            On.GameManager.ReturnToMainMenu -= GameManager_ReturnToMainMenu;
        }

        private IEnumerator GameManager_ReturnToMainMenu(On.GameManager.orig_ReturnToMainMenu orig, GameManager self, GameManager.ReturnToMainMenuSaveModes saveMode, Action<bool> callback)
        {
            MainMenuOrQuit?.Invoke();
            return orig(self, saveMode, callback);
        }

        private void GameManager_OnApplicationQuit(On.GameManager.orig_OnApplicationQuit orig, GameManager self)
        {
            MainMenuOrQuit?.Invoke();
            orig(self);
        }

        private void GameManager_Update(On.GameManager.orig_Update orig, GameManager self)
        {
            orig(self);

            if (Input.GetKeyDown(KeyCode.G))
            {
            }
        }
    }
}