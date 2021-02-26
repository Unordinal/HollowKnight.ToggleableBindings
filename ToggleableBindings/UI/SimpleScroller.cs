#nullable enable

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ToggleableBindings.UI
{
    /// <summary>
    /// Adds a simple scrolling menu by using a containing RectTransform along with a child 'content' RectTransform.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SimpleScroller : MonoBehaviour
    {
        [NotNull] private RectTransform? _scrollTransform;
        [NotNull, SerializeField] private RectTransform? _content;
        [NotNull] private Hashtable? _tweenArgs;

        private GameObject? _lastSelected;

        public RectTransform Content
        {
            get => _content;
            set => _content = value;
        }

        private void Awake()
        {
            _scrollTransform = GetComponent<RectTransform>();
            _tweenArgs = new()
            {
                ["name"] = nameof(SimpleScroller),
                ["time"] = 0.15f,
                ["easetype"] = iTween.EaseType.easeOutSine,
                ["onupdate"] = nameof(ScrollToInstant),
                ["onupdatetarget"] = gameObject
            };
        }

        private void Update()
        {
            GameObject? selected = EventSystem.current.currentSelectedGameObject;
            if (_content == null || selected == null || selected == _lastSelected)
                return;

            if (selected.transform.parent != _content)
                return;

            float contentPosX = _content.anchoredPosition.x;
            var selectedTransform = (RectTransform)selected.transform;

            // The position of the selected UI element is the anchor position -
            // which is the local position within the RectTransform - its height
            // divided by 2 if scrolling down.
            float selectedPosY = Mathf.Abs(selectedTransform.anchoredPosition.y) - (selectedTransform.rect.height / 2);

            // The upper bound of the scrolling content.
            float scrollMinY = _content.anchoredPosition.y;
            // The lower bound of the scrolling content.
            float scrollMaxY = _content.anchoredPosition.y + _scrollTransform.rect.height;

            // Selected position + selected object's height is below the lower bound of the content.
            if (selectedPosY + selectedTransform.rect.height > scrollMaxY)
            {
                selectedPosY -= _scrollTransform.rect.height - selectedTransform.rect.height;
                //_content.anchoredPosition = new Vector2(contentPosX, selectedPosY);
                ScrollTo(new Vector2(contentPosX, selectedPosY));
            }
            // Selected position is above the upper bound of the content.
            else if (selectedPosY < scrollMinY)
            {
                //_content.anchoredPosition = new Vector2(contentPosX, selectedPosY);
                ScrollTo(new Vector2(contentPosX, selectedPosY));
            }

            _lastSelected = selected;
        }

        public void Reset()
        {
            _content.anchoredPosition = new Vector2(_content.anchoredPosition.x, 0f);
            _lastSelected = null;
        }

        private void ScrollTo(Vector2 targetPos)
        {
            if (iTween.Count(gameObject) > 0)
                iTween.Stop(gameObject);

            _tweenArgs["from"] = _content.anchoredPosition;
            _tweenArgs["to"] = targetPos;

            iTween.ValueTo(gameObject, _tweenArgs);
        }

        private void ScrollToInstant(Vector2 targetPos)
        {
            _content.anchoredPosition = targetPos;
        }
    }
}