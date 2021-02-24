#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GlobalEnums;
using ToggleableBindings.Utility;
using UnityEngine;

namespace ToggleableBindings.UI
{
    internal class BindingsUIController : MonoBehaviour
    {
        [NotNull] public static BindingsUIController? Instance { get; private set; }

        private static GameObject _container = null!;

        private BindingsUI _bindingsUI = null!;
        private tk2dSpriteAnimator _spriteAnimator = null!;

        private tk2dSpriteAnimator SpriteAnimator => _spriteAnimator = _spriteAnimator != null ? _spriteAnimator : _spriteAnimator = HeroController.instance.GetComponent<tk2dSpriteAnimator>();

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
            var bindingsUIGO = ObjectFactory.Instantiate(BindingsUI.Prefab, _container, InstanceFlags.DontDestroyOnLoad);
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
            var gmi = GameManager.instance;
            if (!gmi || gmi.gameState is not GameState.PLAYING and not GameState.PAUSED)
                return;

            var hci = HeroController.instance;
            if (!hci)
                return;

            if (hci.CanTalk())
            {
                var inputActions = gmi.inputHandler.inputActions;
                if (inputActions.down.IsPressed && inputActions.superDash.IsPressed)
                {
                    _bindingsUI.Setup(BindingManager.RegisteredBindings.Values);
                    Show();
                }
            }
        }

        private void Show()
        {
            var pdi = PlayerData.instance;
            if (pdi == null)
                return;

            pdi.SetBool("disablePause", true);

            var hci = HeroController.instance;
            if (!hci)
                return;

            hci.RelinquishControl();
            hci.StopAnimationControl();
            SpriteAnimator.Play("Map Open");
            SpriteAnimator.AnimationCompleted = (_, _) => SpriteAnimator.Play("Map Idle");

            _bindingsUI.Show();
        }

        private void Hide()
        {
            if (!HeroController.instance)
            {
                HideInternal();
                return;
            }

            SpriteAnimator.Play("Map Away");
            SpriteAnimator.AnimationCompleted = (_, _) => HideInternal();
        }

        private void HideInternal()
        {
            var pdi = PlayerData.instance;
            if (pdi != null)
                pdi.SetBool("disablePause", false);

            var hci = HeroController.instance;
            if (hci)
            {
                hci.RegainControl();
                hci.StartAnimationControl();
                hci.PreventCastByDialogueEnd();
            }
        }
    }
}