#nullable enable

using System;
using ToggleableBindings.Extensions;
using ToggleableBindings.UI;
using ToggleableBindings.VanillaBindings;
using UnityEngine;
using Vasi;

using UObject = UnityEngine.Object;

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

        private System.Collections.IEnumerator GameManager_ReturnToMainMenu(On.GameManager.orig_ReturnToMainMenu orig, GameManager self, GameManager.ReturnToMainMenuSaveModes saveMode, Action<bool> callback)
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
            if (Input.GetKeyDown(KeyCode.H))
            {
                if (!BindingManager.TryGetBinding<NailBinding>(out var binding))
                    return;

                if (binding.IsApplied)
                    binding.Restore();
                else
                    binding.Apply();
            }

            if (Input.GetKeyDown(KeyCode.J))
            {
                if (!BindingManager.TryGetBinding<ShellBinding>(out var binding))
                    return;

                if (binding.IsApplied)
                    binding.Restore();
                else
                    binding.Apply();
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                if (!BindingManager.TryGetBinding<SoulBinding>(out var binding))
                    return;

                if (binding.IsApplied)
                    binding.Restore();
                else
                    binding.Apply();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                if (!BindingManager.TryGetBinding<CharmsBinding>(out var binding))
                    return;

                if (binding.IsApplied)
                    binding.Restore();
                else
                    binding.Apply();
            }
            
            if (Input.GetKeyDown(KeyCode.Semicolon))
            {
                if (!BindingManager.TryGetBinding("TestNewBinding::SpeedBinding", out var binding))
                    return;

                if (binding.IsApplied)
                    binding.Restore();
                else
                    binding.Apply();
            }
        }
    }
}