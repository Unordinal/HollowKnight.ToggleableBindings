#nullable enable

using System.Diagnostics.CodeAnalysis;
using TMPro;
using ToggleableBindings.Extensions;
using ToggleableBindings.UI;
using ToggleableBindings.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace ToggleableBindings
{
    internal static class CustomPrefabs
    {
        [NotNull] public static FakePrefab? BindingScroller { get; private set; }

        [NotNull] public static FakePrefab? BindingApplyMsg { get; private set; }

        public static FakePrefab BindingsUI => UI.BindingsUI.Prefab;

        public static FakePrefab BindingButton => BindingsUIBindingButton.Prefab;

        static CustomPrefabs()
        {
            BindingApplyMsg = CreateBindingApplyMsgPrefab();
            BindingScroller = CreateBindingScrollerPrefab();
        }

        private static FakePrefab CreateBindingApplyMsgPrefab()
        {
            return FakePrefab.CreateCopy(BaseGamePrefabs.CharmEquipMsg, nameof(BindingApplyMsg), prefab =>
            {
                prefab.RemoveComponent<PlayMakerFSM>();
                prefab.AddComponent<BindingApplyMsg>();
                prefab.AddComponent<SelectedTemporarily>();

                var buttonInfoGO = prefab.FindChild("Button Info");
                var textGO = prefab.FindChild("Text");

                Object.DestroyImmediate(buttonInfoGO, true);
                textGO.RemoveComponent<ChangeFontByLanguage>();

                var text = textGO.GetComponent<TextMeshPro>();
                text.text = "!Placeholder text - if you're seeing this, something went wrong!";
            });
        }

        private static FakePrefab CreateBindingScrollerPrefab()
        {
            return FakePrefab.Create(nameof(BindingScroller), prefab =>
            {
                //! Scroller
                var scrollerTransform = prefab.AddComponent<RectTransform>();
                var scroller = prefab.AddComponent<SimpleScroller>();
                var scrollerMask = prefab.AddComponent<Mask>();
                prefab.AddComponent<Image>();

                scrollerTransform.anchorMin = new Vector2(0f, 0.5f);
                scrollerTransform.anchorMax = new Vector2(1f, 0.5f);
                scrollerTransform.offsetMin = new Vector2(0f, -216f);
                scrollerTransform.offsetMax = new Vector2(0f, 55f);
                scrollerMask.showMaskGraphic = false;

                //! Content
                var contentGO = ObjectFactory.Create("Content", prefab);
                var contentTransform = contentGO.AddComponent<RectTransform>();
                var contentLayoutGroup = contentGO.AddComponent<VerticalLayoutGroup>();

                contentTransform.anchorMin = new Vector2(0f, 0f);
                contentTransform.anchorMax = new Vector2(1f, 1f);
                contentTransform.offsetMin = new Vector2(100f, 0f);
                contentTransform.offsetMax = new Vector2(-100f, 0f);
                contentLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
                contentLayoutGroup.childControlHeight = false;
                contentLayoutGroup.spacing = 11;

                scroller.Content = contentTransform;
            });
        }
    }
}