#nullable enable

using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using Vasi;
namespace ToggleableBindings
{
    public sealed partial class ToggleableBindings
    {
        internal static event Action? MainMenuOrQuit;

        private void AddHooks()
        {
            On.GameManager.OnApplicationQuit += GameManager_OnApplicationQuit;
            On.GameManager.ReturnToMainMenu += GameManager_ReturnToMainMenu;
            On.PlayMakerFSM.Start += ModifyHealth;

#if DEBUG
            On.GameManager.Update += GameManager_Update;
#endif
        }

        private void ModifyHealth(On.PlayMakerFSM.orig_Start orig, PlayMakerFSM self)
        {
            orig(self);
            if (self is
                {
                    name: "Health",
                    FsmName: "Blue Health Control"
                })
            {
                self.GetState("Wait").AddTransition(HutongGames.PlayMaker.FsmEvent.Finished, "Hive Check");
            }
        }

        private void RemoveHooks()
        {
            On.GameManager.OnApplicationQuit -= GameManager_OnApplicationQuit;
            On.GameManager.ReturnToMainMenu -= GameManager_ReturnToMainMenu;
            On.PlayMakerFSM.Start -= ModifyHealth;

#if DEBUG
            On.GameManager.Update -= GameManager_Update;
#endif
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

#if DEBUG
        private void GameManager_Update(On.GameManager.orig_Update orig, GameManager self)
        {
            orig(self);
        }
#endif
    }
}