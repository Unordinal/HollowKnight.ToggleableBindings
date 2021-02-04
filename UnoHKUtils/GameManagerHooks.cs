#nullable enable

using System;
using System.Collections;

namespace UnoHKUtils
{
    public static partial class GameManagerHooks
    {
        static GameManagerHooks()
        {
            On.GameManager.Awake += GameManager_Awake;
            On.GameManager.Start += GameManager_Start;
            On.GameManager.ChangeToScene += GameManager_ChangeToScene;
            On.GameManager.SetState += GameManager_SetState;
            On.GameManager.LoadGame += GameManager_LoadGame;
            On.GameManager.SaveGame_int_Action1 += GameManager_SaveGame;
            On.GameManager.ReturnToMainMenu += GameManager_ReturnToMainMenu;
            On.GameManager.OnApplicationQuit += GameManager_OnApplicationQuit;
        }

        private static void GameManager_Awake(On.GameManager.orig_Awake orig, GameManager self)
        {
            BeforeAwake?.Invoke(self);
            orig(self);
            AfterAwake?.Invoke(self);

            self.StartCoroutine(WaitOneFrame(() => FrameAfterAwake?.Invoke(self)));
        }

        private static void GameManager_Start(On.GameManager.orig_Start orig, GameManager self)
        {
            BeforeStart?.Invoke(self);
            orig(self);
            AfterStart?.Invoke(self);

            self.StartCoroutine(WaitOneFrame(() => FrameAfterStart?.Invoke(self)));
        }

        private static void GameManager_ChangeToScene(On.GameManager.orig_ChangeToScene orig, GameManager self, string targetScene, string entryGateName, float pauseBeforeEnter)
        {
            BeforeChangeToScene?.Invoke(self, targetScene, entryGateName, pauseBeforeEnter);
            orig(self, targetScene, entryGateName, pauseBeforeEnter);
            AfterChangeToScene?.Invoke(self, targetScene, entryGateName, pauseBeforeEnter);

            self.StartCoroutine(WaitOneFrame(() => FrameAfterChangeToScene?.Invoke(self, targetScene, entryGateName, pauseBeforeEnter)));
        }

        private static void GameManager_SetState(On.GameManager.orig_SetState orig, GameManager self, GlobalEnums.GameState newState)
        {
            BeforeSetState?.Invoke(self, newState);
            orig(self, newState);
            AfterSetState?.Invoke(self, newState);

            self.StartCoroutine(WaitOneFrame(() => FrameAfterSetState?.Invoke(self, newState)));
        }

        private static void GameManager_SaveGame(On.GameManager.orig_SaveGame_int_Action1 orig, GameManager self, int saveSlot, Action<bool> callback)
        {
            BeforeSaveGame?.Invoke(self, saveSlot, callback);
            orig(self, saveSlot, callback);
            AfterSaveGame?.Invoke(self, saveSlot, callback);

            self.StartCoroutine(WaitOneFrame(() => FrameAfterSaveGame?.Invoke(self, saveSlot, callback)));
        }

        private static void GameManager_LoadGame(On.GameManager.orig_LoadGame orig, GameManager self, int saveSlot, Action<bool> callback)
        {
            BeforeLoadGame?.Invoke(self, saveSlot, callback);
            orig(self, saveSlot, callback);
            AfterLoadGame?.Invoke(self, saveSlot, callback);

            self.StartCoroutine(WaitOneFrame(() => FrameAfterLoadGame?.Invoke(self, saveSlot, callback)));
        }

        private static IEnumerator GameManager_ReturnToMainMenu(On.GameManager.orig_ReturnToMainMenu orig, GameManager self, GameManager.ReturnToMainMenuSaveModes saveMode, Action<bool> callback)
        {
            BeforeReturnToMainMenu?.Invoke(self, saveMode, callback);
            yield return orig(self, saveMode, callback);
            AfterReturnToMainMenu?.Invoke(self, saveMode, callback);

            self.StartCoroutine(WaitOneFrame(() => FrameAfterReturnToMainMenu?.Invoke(self, saveMode, callback)));
        }

        private static void GameManager_OnApplicationQuit(On.GameManager.orig_OnApplicationQuit orig, GameManager self)
        {
            BeforeOnApplicationQuit?.Invoke(self);
            orig(self);
            AfterOnApplicationQuit?.Invoke(self);

            self.StartCoroutine(WaitOneFrame(() => FrameAfterOnApplicationQuit?.Invoke(self)));
        }

        private static IEnumerator WaitOneFrame(Action? executeOnDone = null)
        {
            yield return null;
            executeOnDone?.Invoke();
        }
    }
}