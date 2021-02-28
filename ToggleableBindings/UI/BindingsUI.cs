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
using Object = UnityEngine.Object;

namespace ToggleableBindings.UI
{
    public class BindingsUI : MonoBehaviour
    {
        internal static FakePrefab Prefab { get; }

        public event Action<IEnumerable<Binding>>? Applied;
        public event Action? Hidden;

        [SerializeField]
        private Selectable _applyButton = null!;

        [SerializeField]
        private SimpleScroller _buttonsScroller = null!;

        [SerializeField]
        private MenuButtonList _buttonsList = null!;

        private Animator _animator = null!;
        private AudioSource _audioSource = null!;
        private CanvasGroup _canvasGroup = null!;

        private readonly List<BindingsUIBindingButton> _buttons = new();
        
        static BindingsUI()
        {
            Prefab = FakePrefab.CreateCopy(BaseGamePrefabs.ChallengeDoorCanvas, nameof(BindingsUI), prefab =>
            {
                //! Find children and component vars
                var panelGO = prefab.FindChild("Panel");

                Assert.IsNotNull(panelGO);
                var beginButtonGO = panelGO.FindChild("BeginButton");
                var buttonsGO = panelGO.FindChild("Buttons");
                var textSuperGO = panelGO.FindChild("Title_Super_Text");
                var textMainGO = panelGO.FindChild("Title_Main_Text");
                var textDescGO = panelGO.FindChild("Description_Text");

                Assert.IsNotNull(beginButtonGO);
                var textBeginGO = beginButtonGO.FindChild("Text");

                //! Assert the rest
                Assert.IsNotNull(buttonsGO);
                Assert.IsNotNull(textSuperGO);
                Assert.IsNotNull(textMainGO);
                Assert.IsNotNull(textDescGO);

                //! Destroy unneeded objects
                DestroyImmediate(buttonsGO, true);

                //! Create additional objects
                var scrollerGO = ObjectFactory.Instantiate(CustomPrefabs.BindingScroller, panelGO);

                //! Add components
                beginButtonGO.AddComponent<EventPropagator>();
                var audioSource = prefab.AddComponent<AudioSource>();
                var bindingsUI = prefab.AddComponent<BindingsUI>();
                var menuButtonList = prefab.AddComponent<MenuButtonList>();

                //! Set variables
                audioSource.volume = Mathf.Clamp01(GameManager.instance.GetImplicitCinematicVolume() / 2f);
                beginButtonGO.name = "ApplyButton";

                textSuperGO.GetComponent<Text>().text = nameof(ToggleableBindings);
                textMainGO.GetComponent<Text>().text = "MENU";
                textDescGO.GetComponent<Text>().text = "Apply and Restore Bindings";
                textBeginGO.GetComponent<Text>().text = "APPLY";

                bindingsUI._applyButton = beginButtonGO.GetComponent<Selectable>();
                bindingsUI._buttonsScroller = scrollerGO.GetComponent<SimpleScroller>();
                bindingsUI._buttonsList = menuButtonList;

                //! Remove components
                prefab.RemoveComponent<BossDoorChallengeUI>();
                beginButtonGO.RemoveComponent<EventTrigger>();
                textBeginGO.RemoveComponent<AutoLocalizeTextUI>();
            });
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();
            _canvasGroup = GetComponent<CanvasGroup>();

            var applyButtonPropagator = _applyButton.GetComponent<EventPropagator>();
            var submitEntry = new EventTrigger.Entry();
            submitEntry.callback.AddListener((data) => Apply());
            submitEntry.eventID = EventTriggerType.Submit;

            var cancelEntry = new EventTrigger.Entry();
            cancelEntry.callback.AddListener((data) => Hide());
            cancelEntry.eventID = EventTriggerType.Cancel;

            var pointerClickEntry = new EventTrigger.Entry();
            pointerClickEntry.callback.AddListener((data) => Apply());
            pointerClickEntry.eventID = EventTriggerType.PointerClick;

            applyButtonPropagator.triggers.Add(submitEntry);
            applyButtonPropagator.triggers.Add(cancelEntry);
            applyButtonPropagator.triggers.Add(pointerClickEntry);
        }

        private void Start()
        {
            GetComponent<Canvas>().worldCamera = GameCameras.instance.hudCamera;
            _canvasGroup.alpha = 0f;
        }

        public void Setup(IEnumerable<Binding> bindings)
        {
            foreach (var button in _buttons)
                DestroyImmediate(button.gameObject);

            _buttons.Clear();
            _buttonsList.ClearSelectables();

            foreach (var binding in bindings)
            {
                var button = CreateButton(binding);

                _buttons.Add(button);
                _buttonsList.AddSelectable(button.GetComponent<Selectable>());
            }

            _buttonsList.AddSelectable(_applyButton);
            _buttonsList.RecalculateNavigation();
        }

        public void Apply()
        {
            Applied?.Invoke(_buttons.Where(b => b.Binding != null && b.IsSelected).Select(b => b.Binding!));
            Hide();
        }

        public void Show()
        {
            if (_buttons.Count == 0)
            {
                ToggleableBindings.Instance.LogError("Show() was called but the BindingsUI was not set up properly!");
                return;
            }

            _canvasGroup.interactable = false;

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

            _canvasGroup.interactable = true;
            InputHandler.Instance.StartUIInput();
        }

        public void Hide()
        {
            GameObject? selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                MenuButton menuButton = selected.GetComponent<MenuButton>();
                if (menuButton)
                    menuButton.ForceDeselect();
            }

            StartCoroutine(HideSequence());
        }

        private IEnumerator HideSequence()
        {
            if (_animator != null)
            {
                _animator.Play("Close");
                yield return null;
                yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
            }

            Hidden?.Invoke();

            FSMUtility.SendEventToGameObject(GameCameras.instance.hudCanvas, "IN");
            gameObject.SetActive(false);
        }

        private BindingsUIBindingButton CreateButton(Binding binding)
        {
            var buttonGO = ObjectFactory.Instantiate(CustomPrefabs.BindingButton, _buttonsScroller.Content.gameObject);
            buttonGO.name = $"{nameof(BindingsUI)}::[{binding.ID}Button]";

            var button = buttonGO.GetComponent<BindingsUIBindingButton>();
            button.AudioSource = _audioSource;
            button.Canceled += Hide;

            button.Setup(binding);

            return button;
        }
    }
}