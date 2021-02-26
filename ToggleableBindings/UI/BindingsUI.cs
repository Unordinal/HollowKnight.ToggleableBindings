#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToggleableBindings.Debugging;
using ToggleableBindings.Extensions;
using ToggleableBindings.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ToggleableBindings.UI
{
    public class BindingsUI : MonoBehaviour
    {
        internal static FakePrefab Prefab { get; }

        public event Action<IEnumerable<Binding>>? Applied;
        public event Action? BeforeHidden;
        public event Action? Hidden;

        private Animator _animator = null!;
        private Canvas _canvas = null!;
        private CanvasGroup _group = null!;
        private GameObject _applyButton = null!;
        private SimpleScroller _buttonsScroller = null!;
        private GameObject _buttonsContent = null!;
        private MenuButtonList _buttonsList = null!;

        private readonly List<BindingsUIBindingButton> _buttons = new();

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
            beginButtonGO.RemoveComponent<EventTrigger>();
            beginButtonGO.AddComponent<EventPropagator>();
            beginButtonGO.name = "ApplyButton";

            var beginTextGO = beginButtonGO.FindChild("Text");
            beginTextGO.RemoveComponent<AutoLocalizeTextUI>();

            DestroyImmediate(buttonsGO, true);
            var scrollerGO = ObjectFactory.Instantiate(CustomPrefabs.BindingScroller);
            scrollerGO.SetParent(panelGO, false);

            var superText = superTextGO.GetComponent<Text>();
            superText.text = nameof(ToggleableBindings);

            var mainText = mainTextGO.GetComponent<Text>();
            mainText.text = "MENU";

            var descText = descTextGO.GetComponent<Text>();
            descText.text = "Apply and Restore Bindings";

            var beginText = beginTextGO.GetComponent<Text>();
            beginText.text = "APPLY";

            Prefab = new FakePrefab(tempInstance, nameof(BindingsUI), true);

            DestroyImmediate(tempInstance, true);
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _canvas = GetComponent<Canvas>();
            _group = GetComponent<CanvasGroup>();

            var panelGO = gameObject.FindChild("Panel");
            _applyButton = panelGO.FindChild("ApplyButton");
            _buttonsList = panelGO.GetComponent<MenuButtonList>();

            var buttonsScrollerGO = panelGO.FindChild(nameof(CustomPrefabs.BindingScroller));
            _buttonsScroller = buttonsScrollerGO.GetComponent<SimpleScroller>();
            _buttonsContent = buttonsScrollerGO.FindChild("Content");

            var beginTrigger = _applyButton.GetComponent<EventPropagator>();
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
            Assert.IsNotNull(_buttonsList, $"{nameof(_buttonsList)} was null - was Setup() called before Awake() had a chance to be called, such as on an inactive object?");

            foreach (var button in _buttons)
                DestroyImmediate(button.gameObject);

            _buttons.Clear();
            _buttonsList.ClearSelectables();
            foreach (var binding in bindings)
            {
                var bindingUIButtonGO = ObjectFactory.Instantiate(BindingsUIBindingButton.Prefab);
                bindingUIButtonGO.name = nameof(BindingsUI) + "::" + binding.Name + "Button";
                bindingUIButtonGO.SetParent(_buttonsContent, false);

                var bindingUIButton = bindingUIButtonGO.GetComponent<BindingsUIBindingButton>();
                bindingUIButton.Canceled += Hide;
                bindingUIButton.Setup(binding);

                _buttons.Add(bindingUIButton);
                _buttonsList.AddSelectable(bindingUIButtonGO.GetComponent<Selectable>());
            }

            _buttonsList.AddSelectable(_applyButton.GetComponent<Selectable>());
            _buttonsList.RecalculateNavigation();
        }

        public void Apply()
        {
            Applied?.Invoke(_buttons.Where(b => b.Binding != null && b.IsSelected).Select(b => b.Binding!));
            Hide();
        }

        public void Show()
        {
            Assert.IsNotNull(_buttonsScroller, nameof(_buttonsScroller));

            if (_buttons.Count == 0)
            {
                ToggleableBindings.Instance.LogError("Show() was called but the BindingsUI was not set up properly!");
                return;
            }

            _group.interactable = false;
            _buttonsScroller.Reset();
            EventSystem.current.SetSelectedGameObject(null);
            gameObject.SetActive(true);

            StartCoroutine(ShowSequence());
            FSMUtility.SendEventToGameObject(GameCameras.instance.hudCanvas, "OUT");
        }

        private IEnumerator ShowSequence()
        {
            yield return null;

            if (_animator != null)
            {
                _animator.Play("Open");
                yield return null;
                yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
            }

            _group.interactable = true;
            /*if (_buttons.Count > 0)
                EventSystem.current.SetSelectedGameObject(_buttons[0].gameObject);
            else
                EventSystem.current.SetSelectedGameObject(_applyButton);*/

            InputHandler.Instance.StartUIInput();
        }

        public void Hide()
        {
            BeforeHidden?.Invoke();

            GameObject? selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                MenuButton menuButton = selected.GetComponent<MenuButton>();
                if (menuButton)
                    menuButton.ForceDeselect();
            }

            StartCoroutine(HideSequence(true));
        }

        private IEnumerator HideSequence(bool sendEvent)
        {
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