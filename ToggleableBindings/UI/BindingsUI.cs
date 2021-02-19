#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modding;
using ToggleableBindings.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ToggleableBindings.UI
{
    public class BindingsUI : MonoBehaviour
    {
        public event Action<IEnumerable<string>>? Submitted;
        public event Action? Hidden;

        private Animator _animator = null!;
        private Canvas _canvas = null!;
        private CanvasGroup _group = null!;
        private GameObject _buttonsGroup = null!;
        private AudioSource _audioSource = null!; 
        private readonly List<(GameObject GO, BindingsUIBindingButton Button)> _buttons = new();

        internal static GameObject GetPrefab()
        {
            var prefab = Prefabs.Instantiate(Prefabs.VanillaChallengeUI, Prefabs.InstanceFlags.StartInactive);
            prefab.name = nameof(BindingsUI);
            var bindingsUI = prefab.AddComponent<BindingsUI>();
            Destroy(prefab.GetComponent<BossDoorChallengeUI>());

            var panelGO = prefab.FindChildByPath("Panel");
            var superTextGO = panelGO.FindChildByPath("Title_Super_Text");
            var mainTextGO = panelGO.FindChildByPath("Title_Main_Text");
            var descriptionTextGO = panelGO.FindChildByPath("Description_Text");

            var superText = superTextGO.GetComponent<Text>();
            superText.text = nameof(ToggleableBindings);

            var mainText = mainTextGO.GetComponent<Text>();
            mainText.text = "Bindings Menu";

            var descriptionText = descriptionTextGO.GetComponent<Text>();
            descriptionText.text = "Apply and Restore Bindings";

            var buttonsGroupGO = prefab.FindChildByPath("Panel/Buttons");
            buttonsGroupGO.transform.DetachChildren();

            var beginGO = panelGO.FindChildByPath("BeginButton");
            var beginTextGO = beginGO.FindChildByPath("Text");
            Destroy(beginTextGO.GetComponent<AutoLocalizeTextUI>());

            var beginText = beginTextGO.GetComponent<Text>();
            beginText.text = "APPLY";

            bindingsUI._audioSource = beginGO.GetComponent<AudioSource>();

            return prefab;
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _canvas = GetComponent<Canvas>();
            _group = GetComponent<CanvasGroup>();
            _buttonsGroup = gameObject.FindChildByPath("Panel/Buttons");
        }

        private void OnEnable()
        {
            foreach (var (_, button) in _buttons)
                button.Reset();
        }

        private void Start()
        {
            _canvas.worldCamera = GameCameras.instance.hudCamera;
            _group.alpha = 0f;
        }

        public void Setup(IEnumerable<Binding> bindings)
        {
            foreach (var (go, _) in _buttons)
                Destroy(go);

            _buttons.Clear();
            foreach (var binding in bindings)
            {
                var bindingUIButtonGO = BindingsUIBindingButton.CreateInstance(binding.ID, binding.Name, binding.DefaultSprite, binding.SelectedSprite);
                var bindingUIButton = bindingUIButtonGO.GetComponent<BindingsUIBindingButton>();
                bindingUIButtonGO.SetParent(_buttonsGroup, false);

                bindingUIButton.Canceled += Hide;
                bindingUIButtonGO.SetActive(true);
                _buttons.Add((bindingUIButtonGO, bindingUIButton));
            }
        }

        public void Apply()
        {
            Submitted?.Invoke(_buttons.Select(b => b.Button.BindingID));
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

            if (_animator is not null)
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
            StartCoroutine(HideSequence(true));
        }

        private IEnumerator HideSequence(bool sendEvent)
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected is not null)
            {
                MenuButton menuButton = selected.GetComponent<MenuButton>();
                if (menuButton is not null)
                    menuButton.ForceDeselect();
            }

            if (_animator is not null)
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