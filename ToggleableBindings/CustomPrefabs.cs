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
            var tempInstance = ObjectFactory.Instantiate(BaseGamePrefabs.CharmEquipMsg);
            tempInstance.RemoveComponent<PlayMakerFSM>();
            tempInstance.AddComponent<BindingApplyMsg>();
            tempInstance.AddComponent<SelectedTemporarily>();

            var buttonInfoGO = tempInstance.FindChild("Button Info");
            Object.DestroyImmediate(buttonInfoGO, true);

            var textGO = tempInstance.FindChild("Text");
            textGO.RemoveComponent<ChangeFontByLanguage>();

            var text = textGO.GetComponent<TextMeshPro>();
            text.text = "!Placeholder text - if you're seeing this, something went wrong!";

            var prefab = new FakePrefab(tempInstance, nameof(BindingApplyMsg));
            Object.DestroyImmediate(tempInstance, true);
            return prefab;
        }

        private static FakePrefab CreateBindingScrollerPrefab()
        {
            // Scroller
            var scrollerGO = ObjectFactory.Create("Scroller");
            var scrollerTransform = scrollerGO.AddComponent<RectTransform>();
            var scroller = scrollerGO.AddComponent<SimpleScroller>();
            var scrollerMask = scrollerGO.AddComponent<Mask>();
            scrollerGO.AddComponent<Image>();

            scrollerTransform.anchorMin = new Vector2(0f, 0.5f);
            scrollerTransform.anchorMax = new Vector2(1f, 0.5f);
            scrollerTransform.offsetMin = new Vector2(0f, -216f);
            scrollerTransform.offsetMax = new Vector2(0f, 55f);
            scrollerMask.showMaskGraphic = false;

            // Content
            var contentGO = ObjectFactory.Create("Content");
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
            contentGO.transform.SetParent(scrollerGO.transform, false);

            var prefab = new FakePrefab(scrollerGO, nameof(BindingScroller));
            Object.DestroyImmediate(scrollerGO, true);
            return prefab;
        }
    }
}