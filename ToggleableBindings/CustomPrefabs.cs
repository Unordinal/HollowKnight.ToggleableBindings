#nullable enable

using System.Diagnostics.CodeAnalysis;
using ToggleableBindings.UI;
using ToggleableBindings.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace ToggleableBindings
{
    internal static class CustomPrefabs
    {
        [NotNull] public static FakePrefab? BindingScroller { get; private set; }

        static CustomPrefabs()
        {
            var bindingScroller = CreateBindingScroller();

            BindingScroller = new FakePrefab(bindingScroller, nameof(BindingScroller), true);

            Object.DestroyImmediate(bindingScroller, true);
        }

        private static GameObject CreateBindingScroller()
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

            //scrollerScrollRect.content = contentTransform;
            scroller.Content = contentTransform;
            contentGO.transform.SetParent(scrollerGO.transform, false);
            return scrollerGO;
        }
    }
}