using System.Reflection;
using ToggleableBindings.Extensions;
using ToggleableBindings.Utility;
using UnityEngine;

namespace ToggleableBindings.UI
{
    internal class BindingsUIController : MonoBehaviour
    {
        public static BindingsUIController Instance { get; private set; }

        private tk2dSpriteAnimator _spriteAnimator;
        private BindingsUI _bindingsUI;
        private FieldInfo _hciFieldInfo;

        private tk2dSpriteAnimator SpriteAnimator => _spriteAnimator ??= HeroController.instance.GetComponent<tk2dSpriteAnimator>();

        public BindingsUIController()
        {
            if (Instance is not null)
            {
                ToggleableBindings.Instance.LogError($"Tried to make a new '{nameof(BindingsUIController)}' when one already exists!");
                Destroy(this);
                return;
            }

            Instance = this;
        }

        internal static void Initialize()
        {
            GameObject container = new GameObject(nameof(ToggleableBindings) + "::" + nameof(BindingsUIController));
            container.AddComponent<BindingsUIController>();
            DontDestroyOnLoad(container);
            container.SetParent(Prefabs.DDOLHolderGO);
        }

        private void Awake()
        {
            var bindingsUIGO = Prefabs.Instantiate(Prefabs.BindingsUI, Prefabs.InstanceFlags.DontDestroyOnLoad);
            bindingsUIGO.name = nameof(ToggleableBindings) + "::" + nameof(BindingsUI);

            _bindingsUI = bindingsUIGO.GetComponent<BindingsUI>();
            _bindingsUI.Hidden += Hide;
            _bindingsUI.Hide();
        }

        private void Update()
        {
            // Because the 'instance' property spams errors due to its 'get' accessor.
            _hciFieldInfo ??= typeof(HeroController).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);
            var hci = _hciFieldInfo.GetValue(null);
            if (hci is null)
                return;

            if (HeroController.instance.CanTalk())
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
            PlayerData.instance.SetBool("disablePause", true);
            HeroController.instance.RelinquishControl();
            HeroController.instance.StopAnimationControl();
            SpriteAnimator.Play("LookUp");

            _bindingsUI.Show();
        }

        private void Hide()
        {
            PlayerData.instance.SetBool("disablePause", false);
            HeroController.instance.RegainControl();
            HeroController.instance.StartAnimationControl();
            HeroController.instance.PreventCastByDialogueEnd();
            SpriteAnimator.Play();
        }
    }
}