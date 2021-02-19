#nullable enable
#pragma warning disable IDE0051 // Remove unused private members

using System;
using Modding;
using ToggleableBindings.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ToggleableBindings.UI
{
    public class BindingsUIBindingButton : MonoBehaviour, ISubmitHandler, ICancelHandler, IPointerClickHandler
    {
        public event Action? Selected;
        public event Action? Canceled;

        public string BindingID { get; private set; } = null!;
        public bool IsSelected { get; private set; }

        private Image _bindingImage = null!;

        private Sprite? _defaultSprite = null;
        private Sprite? _selectedSprite = null;

        private Animator _iconAnimator = null!;
        private Animator _chainAnimator = null!;

        private AudioSource _audioSourcePrefab = null!;
        private AudioEvent _selectedSound;
        private AudioEvent _deselectedSound;

        internal static GameObject GetPrefab()
        {
            var prefab = Instantiate(Prefabs.VanillaNailButton);
            prefab.name = nameof(BindingsUIBindingButton);
            var button = prefab.AddComponent<BindingsUIBindingButton>();
            var vanillaButton = prefab.GetComponent<BossDoorChallengeUIBindingButton>();

            button._audioSourcePrefab = prefab.AddComponent<AudioSource>();
            button._audioSourcePrefab.volume = GameManager.instance.GetImplicitCinematicVolume();
            button._selectedSound = vanillaButton.selectedSound;
            button._deselectedSound = vanillaButton.deselectedSound;


            Destroy(vanillaButton);

            var bindingTextGO = prefab.FindChildByPath("Text");
            var bindingText = bindingTextGO.GetComponent<Text>();
            bindingText.text = "Binding Name Goes Here";
            Destroy(bindingTextGO.GetComponent<AutoLocalizeTextUI>());

            return prefab;
        }

        public static GameObject CreateInstance(string bindingID, string bindingName, Sprite? defaultIcon, Sprite? selectedIcon)
        {
            var instance = Instantiate(Prefabs.BindingsUIBindingButton);
            var button = instance.GetComponent<BindingsUIBindingButton>();
            var buttonTitle = instance.FindChildByPath("Text").GetComponent<Text>();

            button.BindingID = bindingID;
            button._defaultSprite = defaultIcon;
            button._selectedSprite = selectedIcon;
            buttonTitle.text = bindingName;

            return instance;
        }

        private void Awake()
        {
            var imageGO = gameObject.FindChildByPath("Image");
            _bindingImage = imageGO.GetComponent<Image>();
            _iconAnimator = imageGO.GetComponent<Animator>();

            var chainAnimGO = gameObject.FindChildByPath("Chain_Anim");
            _chainAnimator = chainAnimGO.GetComponent<Animator>();

            if (_bindingImage is not null)
                _bindingImage.sprite = _defaultSprite;
        }

        public void Reset()
        {
            IsSelected = false;
            if (_chainAnimator is not null)
                _chainAnimator.Play("Unbind", 0);

            if (_bindingImage is not null)
            {
                _bindingImage.sprite = _defaultSprite;
                _bindingImage.SetNativeSize();
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            IsSelected = !IsSelected;

            if (_bindingImage is not null)
            {
                _bindingImage.sprite = IsSelected ? _selectedSprite : _defaultSprite;
                _bindingImage.SetNativeSize();
            }

            _iconAnimator?.Play("Select");
            _chainAnimator?.Play(IsSelected ? "Bind" : "Unbind");

            AudioEvent toggleSound = IsSelected ? _selectedSound : _deselectedSound;
            toggleSound.SpawnAndPlayOneShot(_audioSourcePrefab, transform.position);

            GameCameras.instance.cameraShakeFSM.SendEvent("EnemyKillShake");
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