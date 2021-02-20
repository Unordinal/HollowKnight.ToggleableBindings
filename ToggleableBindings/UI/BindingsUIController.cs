#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using GlobalEnums;
using ToggleableBindings.Extensions;
using ToggleableBindings.Utility;
using UnityEngine;

namespace ToggleableBindings.UI
{
    internal class BindingsUIController : MonoBehaviour
    {
        [NotNull] public static BindingsUIController? Instance { get; private set; }

        private BindingsUI _bindingsUI = null!;
        private tk2dSpriteAnimator _spriteAnimator = null!;

        private tk2dSpriteAnimator SpriteAnimator => _spriteAnimator ??= HeroController.instance.GetComponent<tk2dSpriteAnimator>();

        public BindingsUIController()
        {
            if (Instance != null)
            {
                ToggleableBindings.Instance.LogError($"Tried to make a new '{nameof(BindingsUIController)}' when one already exists!");
                Destroy(this);
                return;
            }

            Instance = this;
        }

        internal static void Initialize()
        {
            GameObject container = ObjectFactory.Create(nameof(BindingsUIController), InstanceFlags.DontDestroyOnLoad);
            container.AddComponent<BindingsUIController>();
        }

        private void Awake()
        {
            var bindingsUIGO = ObjectFactory.Instantiate(BindingsUI.Prefab, InstanceFlags.DontDestroyOnLoad);
            bindingsUIGO.name = nameof(BindingsUI);

            _bindingsUI = bindingsUIGO.GetComponent<BindingsUI>();
            _bindingsUI.Hide();
            _bindingsUI.Applied += Applied;
            _bindingsUI.Hidden += Hide;
        }

        private void Applied(IEnumerable<Binding> selectedBindings)
        {
            BindingManager.SetActiveBindings(selectedBindings);
        }

        private void Update()
        {
            if (GameManager.instance.gameState is not GameState.PLAYING and not GameState.PAUSED)
                return;

            var hci = HeroController.instance;
            if (!hci)
                return;

            if (hci.CanTalk())
            {
                var inputActions = GameManager.instance.inputHandler.inputActions;
                if (inputActions.down.IsPressed && inputActions.superDash.IsPressed)
                {
                    _bindingsUI.Setup(BindingManager.RegisteredBindings.Values);
                    Show();
                }
            }
        }

        private void Show()
        {
            PlayerData.instance?.SetBool("disablePause", true);
            if (HeroController.instance != null)
            {
                HeroController.instance.RelinquishControl();
                HeroController.instance.StopAnimationControl();
                SpriteAnimator.Play("Map Open");
                SpriteAnimator.AnimationCompleted = (_, _) => SpriteAnimator.Play("Map Idle");
            }

            _bindingsUI.Show();
        }

        private void Hide()
        {
            if (HeroController.instance != null)
            {
                SpriteAnimator.Play("Map Away");
                SpriteAnimator.AnimationCompleted = (_, _) =>
                {
                    PlayerData.instance?.SetBool("disablePause", false);
                    HeroController.instance.RegainControl();
                    HeroController.instance.StartAnimationControl();
                    HeroController.instance.PreventCastByDialogueEnd();
                };
            }
        }
    }
}