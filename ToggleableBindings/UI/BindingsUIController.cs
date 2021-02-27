#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GlobalEnums;
using ToggleableBindings.Debugging;
using ToggleableBindings.Utility;
using UnityEngine;

namespace ToggleableBindings.UI
{
    internal class BindingsUIController : MonoBehaviour
    {
        [NotNull] public static BindingsUIController? Instance { get; private set; }

        private static GameObject _container = null!;

        private BindingsUI? _bindingsUI = null;

        private tk2dSpriteAnimator? _heroAnimator;

        private tk2dSpriteAnimator HeroAnimator
        {
            get
            {
                if (_heroAnimator != null)
                    return _heroAnimator;

                var hci = HeroController.instance;
                if (hci == null)
                    throw new InvalidOperationException("HeroController is null!");

                return _heroAnimator = hci.GetComponent<tk2dSpriteAnimator>();
            }
        }

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
            _container = ObjectFactory.Create(nameof(BindingsUIController), InstanceFlags.DontDestroyOnLoad);
            _container.AddComponent<BindingsUIController>();
        }

        internal static void Unload()
        {
            if (Instance._bindingsUI != null)
                Instance._bindingsUI.Hide();

            Destroy(_container);
            Instance = null;
        }

        private void Update()
        {
            if (CanOpenMenu())
                SetupAndOpenMenu();
        }

        private bool CanOpenMenu()
        {
            var gmi = GameManager.instance;
            if (gmi == null || gmi.gameState is not GameState.PLAYING and not GameState.PAUSED)
                return false;

            // Separate from above null check as 'HeroController.instance' always logs an error if it's null when you try and retrieve it.
            var hci = HeroController.instance;
            if (hci == null)
                return false;

            if (hci.CanTalk())
            {
                var inputActions = gmi.inputHandler.inputActions;
                if (inputActions.down.IsPressed && inputActions.superDash.IsPressed)
                    return true;
            }

            return false;
        }

        private void SetupAndOpenMenu()
        {
            ToggleableBindings.Instance.LogDebug("Opening BindingsUI...");

            var hci = HeroController.instance;
            if (hci == null)
                throw new InvalidOperationException("Tried to open the BindingsUI when there is no instance of HeroController!");

            var pdi = hci.playerData;
            if (pdi == null)
                throw new InvalidOperationException("Tried to open the BindingsUI when there is no instance of PlayerData!");

            pdi.SetBool("disablePause", true);
            hci.RelinquishControl();
            hci.StopAnimationControl();
            HeroAnimator.Play("Map Open");
            HeroAnimator.AnimationCompleted = (_, _) => HeroAnimator.Play("Map Idle");

            if (_bindingsUI == null)
                CreateUI();

            _bindingsUI.Setup(BindingManager.RegisteredBindings.Values);
            _bindingsUI.Show();
        }

        private void OnApplied(IEnumerable<Binding> selectedBindings)
        {
            BindingManager.SetActiveBindings(selectedBindings);
        }

        private void OnHidden()
        {
            if (HeroController.instance)
            {
                HeroAnimator.Play("Map Away");
                HeroAnimator.AnimationCompleted = (_, _) => AfterOnHidden();
            }
            else
                AfterOnHidden();
        }

        private void AfterOnHidden()
        {
            var pdi = PlayerData.instance;
            if (pdi != null)
                pdi.SetBool("disablePause", false);

            var hci = HeroController.instance;
            var gmi = GameManager.instance;
            if (hci && gmi && gmi.GetPlayerDataBool("atBench") == false)
            {
                hci.RegainControl();
                hci.StartAnimationControl();
                hci.PreventCastByDialogueEnd();
            }
        }

        [MemberNotNull(nameof(_bindingsUI))]
        private void CreateUI()
        {
            var bindingsUIGO = ObjectFactory.Instantiate(BindingsUI.Prefab);
            bindingsUIGO.name = nameof(BindingsUI);

            _bindingsUI = bindingsUIGO.GetComponent<BindingsUI>();
            _bindingsUI.Applied += OnApplied;
            _bindingsUI.Hidden += OnHidden;

            bindingsUIGO.SetActive(false);
        }
    }
}