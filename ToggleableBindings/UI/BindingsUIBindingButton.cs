#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using ToggleableBindings.Extensions;
using ToggleableBindings.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ToggleableBindings.UI
{
    public class BindingsUIBindingButton : MonoBehaviour, ISubmitHandler, ICancelHandler, IPointerClickHandler
    {
        internal static FakePrefab Prefab { get; }

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
            tempInstance.AddComponent<EventPropagator>();

            var bindingTextGO = tempInstance.FindChild("Text");
            bindingTextGO.RemoveComponent<AutoLocalizeTextUI>();

            var bindingText = bindingTextGO.GetComponent<Text>();
            bindingText.text = "<Binding Name>";

            Prefab = new FakePrefab(tempInstance, nameof(BindingsUIBindingButton));
            DestroyImmediate(tempInstance, true);
        }

        private void Awake()
        {
            var chainAnimGO = gameObject.FindChild("Chain_Anim");
            var imageGO = gameObject.FindChild("Image");
            var textGO = gameObject.FindChild("Text");

            _audioSource = GetComponent<AudioSource>();
            _bindingImage = imageGO.GetComponent<Image>();
            _chainAnimator = chainAnimGO.GetComponent<Animator>();
            _iconAnimator = imageGO.GetComponent<Animator>();
            _title = textGO.GetComponent<Text>();
            _bindingImage.sprite = _defaultSprite;
        }

        private void Start()
        {
            var vanillaButtonPrefab = BaseGamePrefabs.NailButton.UnsafeGameObject.GetComponent<BossDoorChallengeUIBindingButton>();

            _audioSource.volume = Mathf.Clamp01(GameManager.instance.GetImplicitCinematicVolume() / 2f);
            _selectedSound = vanillaButtonPrefab.selectedSound;
            _deselectedSound = vanillaButtonPrefab.deselectedSound;

            UpdateState(false);
        }

        public void Setup(Binding binding)
        {
            Binding = binding ?? throw new ArgumentNullException(nameof(binding));
            _defaultSprite = binding.DefaultSprite;
            _selectedSprite = binding.SelectedSprite;
            _title.text = binding.Name.ToUpper();

            IsSelected = binding.IsApplied;
        }

        private void UpdateState(bool playEffects)
        {
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
                    _audioSource.PlayOneShot(selectSound.Clip);
                }
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            var canBeApplied = ToggleableBindings.EnforceBindingRestrictions 
                ? (Binding?.CanBeApplied() ?? false) 
                : true;

            if (canBeApplied.Value)
            {
                IsSelected = !IsSelected;

                UpdateState(true);
                Selected?.Invoke();
            }
            else
            {
                var applyMsgGO = ObjectFactory.Instantiate(CustomPrefabs.BindingApplyMsg);
                var tmpText = applyMsgGO.FindChild("Text").GetComponent<TextMeshPro>();
                tmpText.text = canBeApplied.Information ?? tmpText.text;
            }
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