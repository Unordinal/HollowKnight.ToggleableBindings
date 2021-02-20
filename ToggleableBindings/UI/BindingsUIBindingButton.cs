#nullable enable

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

        [NotNull] public Binding? Binding { get; private set; }

        public bool IsSelected { get; private set; }

        private Image _bindingImage = null!;

        private Sprite? _defaultSprite = null;
        private Sprite? _selectedSprite = null;

        private Animator _iconAnimator = null!;
        private Animator _chainAnimator = null!;

        private AudioSource _audioSource = null!;
        private AudioEvent _selectedSound;
        private AudioEvent _deselectedSound;

        static BindingsUIBindingButton()
        {
            var tempInstance = ObjectFactory.Instantiate(BaseGamePrefabs.NailButton, InstanceFlags.StartInactive);
            tempInstance.RemoveComponent<BossDoorChallengeUIBindingButton>();
            var button = tempInstance.AddComponent<BindingsUIBindingButton>();
            tempInstance.AddComponent<AudioSource>();

            var bindingTextGO = tempInstance.FindChild("Text");
            bindingTextGO.RemoveComponent<AutoLocalizeTextUI>();

            var bindingText = bindingTextGO.GetComponent<Text>();
            bindingText.text = "<Binding Name>";

            Prefab = new FakePrefab(tempInstance, nameof(BindingsUIBindingButton));

            DestroyImmediate(tempInstance, true);
        }

        public static GameObject CreateInstance(Binding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            var instance = ObjectFactory.Instantiate(Prefab);
            var button = instance.GetComponent<BindingsUIBindingButton>();
            var buttonTitle = instance.FindChild("Text").GetComponent<Text>();

            button.Binding = binding;
            button._defaultSprite = binding.DefaultSprite;
            button._selectedSprite = binding.SelectedSprite;
            buttonTitle.text = binding.Name;

            var vanillaButtonPrefab = BaseGamePrefabs.NailButton.UnsafeGameObject.GetComponent<BossDoorChallengeUIBindingButton>();
            button._selectedSound = vanillaButtonPrefab.selectedSound;
            button._deselectedSound = vanillaButtonPrefab.deselectedSound;

            return instance;
        }

        private void Awake()
        {
            ToggleableBindings.Instance.Log(nameof(BindingsUIBindingButton) + "::" + gameObject.name + " - Awake");
            var imageGO = gameObject.FindChild("Image");
            _bindingImage = imageGO.GetComponent<Image>();
            _iconAnimator = imageGO.GetComponent<Animator>();

            var chainAnimGO = gameObject.FindChild("Chain_Anim");
            _chainAnimator = chainAnimGO.GetComponent<Animator>();

            _audioSource = GetComponent<AudioSource>();

            if (_bindingImage)
                _bindingImage.sprite = _defaultSprite;
        }

        private void Start()
        {
            ToggleableBindings.Instance.Log(nameof(BindingsUIBindingButton) + "::" + gameObject.name + " - Start");

            _audioSource.volume = GameManager.instance.GetImplicitCinematicVolume();
            IsSelected = Binding.IsApplied;

            SetState(IsSelected, true);
        }

        public void SetState(bool selected, bool isInstant)
        {
            ToggleableBindings.Instance.Log(nameof(BindingsUIBindingButton) + "::" + gameObject.name + " - SetState");
            float time = isInstant ? 1f : 0f;
            IsSelected = selected;

            if (_bindingImage != null)
            {
                _bindingImage.sprite = IsSelected ? _selectedSprite : _defaultSprite;
                _bindingImage.SetNativeSize();
            }

            if (_iconAnimator)
                _iconAnimator.Play("Select", -1, time);

            if (_chainAnimator)
                _chainAnimator.Play(IsSelected ? "Bind" : "Unbind", -1, time);

            if (!isInstant)
            {
                AudioEvent toggleSound = IsSelected ? _selectedSound : _deselectedSound;
                toggleSound.SpawnAndPlayOneShot(_audioSource, transform.position);

                GameCameras.instance.cameraShakeFSM.SendEvent("EnemyKillShake");
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            ToggleableBindings.Instance.Log(nameof(BindingsUIBindingButton) + "::" + gameObject.name + " - Submit");
            SetState(!IsSelected, false);
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