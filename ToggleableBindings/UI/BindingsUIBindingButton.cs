﻿#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using ToggleableBindings.Extensions;
using ToggleableBindings.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ToggleableBindings.UI
{
    public class BindingsUIBindingButton : MonoBehaviour, ISubmitHandler, ICancelHandler, IPointerClickHandler
    {
        public static FakePrefab Prefab { get; }

        public event Action? Selected;
        public event Action? Canceled;

        public Binding? Binding { get; private set; }

        public bool IsSelected { get; private set; }

        private Text _title = null!;

        private Image _bindingImage = null!;
        private Sprite? _defaultSprite;
        private Sprite? _selectedSprite;

        private Animator _iconAnimator = null!;
        private Animator _chainAnimator = null!;

        private AudioSource _audioSource = null!;
        private AudioEvent _selectedSound;
        private AudioEvent _deselectedSound;

        static BindingsUIBindingButton()
        {
            var tempInstance = ObjectFactory.Instantiate(BaseGamePrefabs.NailButton, InstanceFlags.StartInactive);
            tempInstance.RemoveComponent<BossDoorChallengeUIBindingButton>();
            tempInstance.AddComponent<BindingsUIBindingButton>();
            tempInstance.AddComponent<AudioSource>();

            var bindingTextGO = tempInstance.FindChild("Text");
            bindingTextGO.RemoveComponent<AutoLocalizeTextUI>();

            var bindingText = bindingTextGO.GetComponent<Text>();
            bindingText.text = "<Binding Name>";

            Prefab = new FakePrefab(tempInstance, nameof(BindingsUIBindingButton));
            DestroyImmediate(tempInstance, true);
        }

        private void Awake()
        {
            ToggleableBindings.Instance.Log(nameof(BindingsUIBindingButton) + "::" + gameObject.name + " - " + nameof(Awake));
            var chainAnimGO = gameObject.FindChild("Chain_Anim");
            var imageGO = gameObject.FindChild("Image");
            var textGO = gameObject.FindChild("Text");

            _audioSource = GetComponent<AudioSource>();
            _bindingImage = imageGO.GetComponent<Image>();
            _chainAnimator = chainAnimGO.GetComponent<Animator>();
            _iconAnimator = imageGO.GetComponent<Animator>();
            _title = textGO.GetComponent<Text>();

            if (_bindingImage)
                _bindingImage.sprite = _defaultSprite;
        }

        private void Start()
        {
            ToggleableBindings.Instance.Log(nameof(BindingsUIBindingButton) + "::" + gameObject.name + " - " + nameof(Start));
            var vanillaButtonPrefab = BaseGamePrefabs.NailButton.UnsafeGameObject.GetComponent<BossDoorChallengeUIBindingButton>();

            _audioSource.volume = GameManager.instance.GetImplicitCinematicVolume();
            _selectedSound = vanillaButtonPrefab.selectedSound;
            _deselectedSound = vanillaButtonPrefab.deselectedSound;
        }

        public void Setup(Binding binding)
        {
            Binding = binding ?? throw new ArgumentNullException(nameof(binding));
            _defaultSprite = binding.DefaultSprite;
            _selectedSprite = binding.SelectedSprite;
            _title.text = binding.Name;

            IsSelected = binding.IsApplied;
            UpdateState(false);
        }

        private void UpdateState(bool playEffects)
        {
            ToggleableBindings.Instance.Log(nameof(BindingsUIBindingButton) + "::" + gameObject.name + " - " + nameof(UpdateState));
            if (Binding == null)
                throw new InvalidOperationException("Setup() was not called on this binding button!");

            float startTime = playEffects ? 0f : 1f;
            if (_bindingImage)
            {
                _bindingImage.sprite = IsSelected ? _selectedSprite : _defaultSprite;
                _bindingImage.SetNativeSize();
            }

            if (_chainAnimator)
                _chainAnimator.Play(IsSelected ? "Bind" : "Unbind", -1, startTime);

            if (_iconAnimator)
                _iconAnimator.Play("Select", -1, startTime);

            if (playEffects)
            {
                GameCameras.instance.cameraShakeFSM.SendEvent("EnemyKillShake");

                if (_audioSource)
                {
                    AudioEvent selectSound = IsSelected ? _selectedSound : _deselectedSound;
                    selectSound.SpawnAndPlayOneShot(_audioSource, transform.position);
                }
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            ToggleableBindings.Instance.Log(nameof(BindingsUIBindingButton) + "::" + gameObject.name + " - " + nameof(OnSubmit));
            IsSelected = !IsSelected;

            UpdateState(true);
            Selected?.Invoke();
        }

        public void OnCancel(BaseEventData eventData)
        {
            Canceled?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSubmit(eventData);
        }
    }
}