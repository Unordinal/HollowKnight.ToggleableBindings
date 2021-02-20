#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToggleableBindings.Extensions;
using ToggleableBindings.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ToggleableBindings.UI
{
    public class BindingsUI : MonoBehaviour
    {
        public static FakePrefab Prefab { get; }

        public event Action<IEnumerable<Binding>>? Applied;
        public event Action? BeforeHidden;
        public event Action? Hidden;

        private Animator _animator = null!;
        private Canvas _canvas = null!;
        private CanvasGroup _group = null!;
        private GameObject _beginButton = null!;
        private GameObject _buttonsGroup = null!;
        private MenuButtonList _buttonsList = null!;

        private readonly List<(GameObject GO, BindingsUIBindingButton Button)> _buttons = new();

        static BindingsUI()
        {
            var tempInstance = ObjectFactory.Instantiate(BaseGamePrefabs.ChallengeDoorCanvas, InstanceFlags.StartInactive);
            tempInstance.RemoveComponent<BossDoorChallengeUI>();
            var bindingsUI = tempInstance.AddComponent<BindingsUI>();

            var panelGO = tempInstance.FindChild("Panel");
            panelGO.AddComponent<MenuButtonList>();

            var buttonsGO = panelGO.FindChild("Buttons");
            var superTextGO = panelGO.FindChild("Title_Super_Text");
            var mainTextGO = panelGO.FindChild("Title_Main_Text");
            var descTextGO = panelGO.FindChild("Description_Text");
            var beginButtonGO = panelGO.FindChild("BeginButton");
            var beginTextGO = beginButtonGO.FindChild("Text");
            beginTextGO.RemoveComponent<AutoLocalizeTextUI>();

            // Destroy original buttons
            foreach (Transform child in buttonsGO.transform.AsGeneric<Transform>().ToList())
                DestroyImmediate(child.gameObject, true);

            var superText = superTextGO.GetComponent<Text>();
            superText.text = nameof(ToggleableBindings);

            var mainText = mainTextGO.GetComponent<Text>();
            mainText.text = "Bindings Menu";

            var descText = descTextGO.GetComponent<Text>();
            descText.text = "Apply and Restore Bindings";

            var beginText = beginTextGO.GetComponent<Text>();
            beginText.text = "APPLY";

            Prefab = new FakePrefab(tempInstance, nameof(BindingsUI));

            DestroyImmediate(tempInstance, true);
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _canvas = GetComponent<Canvas>();
            _group = GetComponent<CanvasGroup>();

            var panelGO = gameObject.FindChild("Panel");
            _beginButton = panelGO.FindChild("BeginButton");
            _buttonsGroup = panelGO.FindChild("Buttons");
            _buttonsList = panelGO.GetComponent<MenuButtonList>();

            var beginTrigger = _beginButton.GetComponent<EventTrigger>();
            var submitEntry = new EventTrigger.Entry();
            submitEntry.callback.AddListener((data) => Apply());
            submitEntry.eventID = EventTriggerType.Submit;

            var cancelEntry = new EventTrigger.Entry();
            cancelEntry.callback.AddListener((data) => Hide());
            cancelEntry.eventID = EventTriggerType.Cancel;

            var pointerClickEntry = new EventTrigger.Entry();
            pointerClickEntry.callback.AddListener((data) => Apply());
            pointerClickEntry.eventID = EventTriggerType.PointerClick;

            beginTrigger.triggers.Clear();
            beginTrigger.triggers.Add(submitEntry);
            beginTrigger.triggers.Add(cancelEntry);
            beginTrigger.triggers.Add(pointerClickEntry);
        }

        private void Start()
        {
            _canvas.worldCamera = GameCameras.instance.hudCamera;
            _group.alpha = 0f;
        }

        public void Setup(IEnumerable<Binding> bindings)
        {
            foreach (var (go, button) in _buttons)
            {
                button.Canceled -= Hide;
                Destroy(go);
            }

            _buttons.Clear();
            _buttonsList.ClearSelectables();
            foreach (var binding in bindings)
            {
                var bindingUIButtonGO = BindingsUIBindingButton.CreateInstance(binding);
                bindingUIButtonGO.name = nameof(BindingsUI) + "::" + binding.Name + "Button";
                bindingUIButtonGO.SetParent(_buttonsGroup, false);

                var bindingUIButton = bindingUIButtonGO.GetComponent<BindingsUIBindingButton>();

                bindingUIButton.Canceled += Hide;
                _buttons.Add((bindingUIButtonGO, bindingUIButton));

                _buttonsList.AddSelectable(bindingUIButtonGO.GetComponent<Selectable>());
            }

            _buttonsList.AddSelectable(_beginButton.GetComponent<Selectable>());
            _buttonsList.RecalculateNavigation();
        }

        public void Apply()
        {
            Applied?.Invoke(_buttons.Where(b => b.Button.Binding != null && b.Button.IsSelected).Select(b => b.Button.Binding!));
            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            StartCoroutine(ShowSequence());
            FSMUtility.SendEventToGameObject(GameCameras.instance.hudCanvas, "OUT");
        }

        private IEnumerator ShowSequence()
        {
            _group.interactable = false;
            EventSystem.current.SetSelectedGameObject(null);
            yield return null;

            if (_animator != null)
            {
                _animator.Play("Open");
                yield return null;
                yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
            }

            _group.interactable = true;
            if (_buttons.Count > 0)
                EventSystem.current.SetSelectedGameObject(_buttons[0].GO);

            InputHandler.Instance.StartUIInput();
        }

        public void Hide()
        {
            BeforeHidden?.Invoke();
            StartCoroutine(HideSequence(true));
        }

        private IEnumerator HideSequence(bool sendEvent)
        {
            GameObject? selected = EventSystem.current?.currentSelectedGameObject;
            if (selected != null)
            {
                MenuButton menuButton = selected.GetComponent<MenuButton>();
                if (menuButton != null)
                    menuButton.ForceDeselect();
            }

            if (_animator != null)
            {
                _animator.Play("Close");
                yield return null;
                yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
            }

            if (sendEvent)
                Hidden?.Invoke();

            FSMUtility.SendEventToGameObject(GameCameras.instance.hudCanvas, "IN");
            gameObject.SetActive(false);
        }
    }
}