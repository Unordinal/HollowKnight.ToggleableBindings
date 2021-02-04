#nullable enable

using System;

namespace UnoHKUtils
{
    public static partial class GameManagerHooks
    {
        public delegate void AwakeHandler(GameManager gameManager);

        public static event AwakeHandler? BeforeAwake;
        public static event AwakeHandler? AfterAwake;
        public static event AwakeHandler? FrameAfterAwake;

        public delegate void StartHandler(GameManager gameManager);

        public static event StartHandler? BeforeStart;
        public static event StartHandler? AfterStart;
        public static event StartHandler? FrameAfterStart;

        public delegate void ChangeToSceneHandler(GameManager gameManager, string targetScene, string entryGateName, float pauseBeforeEnter);

        public static event ChangeToSceneHandler? BeforeChangeToScene;
        public static event ChangeToSceneHandler? AfterChangeToScene;
        public static event ChangeToSceneHandler? FrameAfterChangeToScene;

        public delegate void SetStateHandler(GameManager gameManager, GlobalEnums.GameState newState);

        public static event SetStateHandler? BeforeSetState;
        public static event SetStateHandler? AfterSetState;
        public static event SetStateHandler? FrameAfterSetState;

        public delegate void LoadGameHandler(GameManager gameManager, int saveSlot, Action<bool> callback);

        public static event LoadGameHandler? BeforeLoadGame;
        public static event LoadGameHandler? AfterLoadGame;
        public static event LoadGameHandler? FrameAfterLoadGame;

        public delegate void SaveGameHandler(GameManager gameManager, int saveSlot, Action<bool> callback);

        public static event SaveGameHandler? BeforeSaveGame;
        public static event SaveGameHandler? AfterSaveGame;
        public static event SaveGameHandler? FrameAfterSaveGame;

        public delegate void ReturnToMainMenuHandler(GameManager gameManager, GameManager.ReturnToMainMenuSaveModes saveMode, Action<bool> callback);

        public static event ReturnToMainMenuHandler? BeforeReturnToMainMenu;
        public static event ReturnToMainMenuHandler? AfterReturnToMainMenu;
        public static event ReturnToMainMenuHandler? FrameAfterReturnToMainMenu;

        public delegate void OnApplicationQuitHandler(GameManager gameManager);

        public static event OnApplicationQuitHandler? BeforeOnApplicationQuit;
        public static event OnApplicationQuitHandler? AfterOnApplicationQuit;
        public static event OnApplicationQuitHandler? FrameAfterOnApplicationQuit;
    }
}