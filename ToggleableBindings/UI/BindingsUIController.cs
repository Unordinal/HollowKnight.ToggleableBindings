#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GlobalEnums;
using ToggleableBindings.Debugging;
using ToggleableBindings.Extensions;
using ToggleableBindings.Utility;
using UnityEngine;

namespace ToggleableBindings.UI
{
    internal class BindingsUIController : MonoBehaviour
    {
        [NotNull] public static BindingsUIController? Instance { get; private set; }

        private static GameObject _container = null!;

        private BindingsUI _bindingsUI = null!;

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
            Instance._bindingsUI.Hide();
            Destroy(_container);
            Instance = null;
        }

        private void Awake()
        {
            Assert.IsNotNull(_container);

            var bindingsUIGO = ObjectFactory.Instantiate(BindingsUI.Prefab, _container);
            bindingsUIGO.name = nameof(BindingsUI);

            _bindingsUI = bindingsUIGO.GetComponent<BindingsUI>();
            _bindingsUI.Hide();
            _bindingsUI.Applied += Applied;
            _bindingsUI.Hidden += StartHide;
        }

        private void Applied(IEnumerable<Binding> selectedBindings)
        {
            BindingManager.SetActiveBindings(selectedBindings);
        }

        private void Update()
        {
            var gmi = GameManager.instance;
            if (gmi == null || gmi.gameState is not GameState.PLAYING and not GameState.PAUSED)
                return;

            // Separate from above null check as 'HeroController.instance' always logs an error if it's null when you try and retrieve it.
            var hci = HeroController.instance;
            if (hci == null)
                return;

            if (hci.CanTalk())
            {
                var inputActions = gmi.inputHandler.inputActions;
                if (inputActions.down.IsPressed && inputActions.superDash.IsPressed)
                {
                    ToggleableBindings.Instance.LogDebug("Opened BindingsUI.");
                    _bindingsUI.Setup(BindingManager.RegisteredBindings.Values);
                    Show();
                }
            }
        }

        private void Show()
        {
            Assert.IsNotNull(_bindingsUI);

            var pdi = PlayerData.instance;
            var hci = HeroController.instance;
            if (pdi == null || hci == null)
                return;

            pdi.SetBool("disablePause", true);
            hci.RelinquishControl();
            hci.StopAnimationControl();
            HeroAnimator.Play("Map Open");
            HeroAnimator.AnimationCompleted = (_, _) => HeroAnimator.Play("Map Idle");

            _bindingsUI.Show();
        }

        private void StartHide()
        {
            if (HeroController.instance)
            {
                HeroAnimator.Play("Map Away");
                HeroAnimator.AnimationCompleted = (_, _) => Hide();
            }
            else
                Hide();
        }

        private void Hide()
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
    }
}