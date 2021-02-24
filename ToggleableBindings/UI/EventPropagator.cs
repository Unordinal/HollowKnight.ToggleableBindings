using UnityEngine.EventSystems;

namespace ToggleableBindings.UI
{
    internal class EventPropagator : EventTrigger
    {
        public bool PropagateOnBeginDrag { get; set; } = true;

        public bool PropagateOnCancel { get; set; } = true;

        public bool PropagateOnDeselect { get; set; } = true;

        public bool PropagateOnDrag { get; set; } = true;

        public bool PropagateOnDrop { get; set; } = true;

        public bool PropagateOnEndDrag { get; set; } = true;

        public bool PropagateOnInitializePotentialDrag { get; set; } = true;

        public bool PropagateOnPointerDown { get; set; } = true;

        public bool PropagateOnPointerEnter { get; set; } = true;

        public bool PropagateOnPointerExit { get; set; } = true;

        public bool PropagateOnPointerUp { get; set; } = true;

        public bool PropagateOnScroll { get; set; } = true;

        public bool PropagateOnSelect { get; set; } = true;

        public bool PropagateOnUpdateSelected { get; set; } = true;

        public bool PropagateOnMove { get; set; } = true;

        public bool PropagateOnPointerClick { get; set; } = true;

        public bool PropagateOnSubmit { get; set; } = true;

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            if (PropagateOnBeginDrag)
                Propagate(eventData, ExecuteEvents.beginDragHandler);
        }

        public override void OnCancel(BaseEventData eventData)
        {
            base.OnCancel(eventData);
            if (PropagateOnCancel)
                Propagate(eventData, ExecuteEvents.cancelHandler);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            if (PropagateOnDeselect)
                Propagate(eventData, ExecuteEvents.deselectHandler);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            if (PropagateOnDrag)
                Propagate(eventData, ExecuteEvents.dragHandler);
        }

        public override void OnDrop(PointerEventData eventData)
        {
            base.OnDrop(eventData);
            if (PropagateOnDrop)
                Propagate(eventData, ExecuteEvents.dropHandler);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            if (PropagateOnEndDrag)
                Propagate(eventData, ExecuteEvents.endDragHandler);
        }

        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            base.OnInitializePotentialDrag(eventData);
            if (PropagateOnInitializePotentialDrag)
                Propagate(eventData, ExecuteEvents.initializePotentialDrag);
        }

        public override void OnMove(AxisEventData eventData)
        {
            base.OnMove(eventData);
            if (PropagateOnMove)
                Propagate(eventData, ExecuteEvents.moveHandler);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            if (PropagateOnPointerClick)
                Propagate(eventData, ExecuteEvents.pointerClickHandler);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (PropagateOnPointerDown)
                Propagate(eventData, ExecuteEvents.pointerDownHandler);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (PropagateOnPointerEnter)
                Propagate(eventData, ExecuteEvents.pointerEnterHandler);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            if (PropagateOnPointerExit)
                Propagate(eventData, ExecuteEvents.pointerExitHandler);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (PropagateOnPointerUp)
                Propagate(eventData, ExecuteEvents.pointerUpHandler);
        }

        public override void OnScroll(PointerEventData eventData)
        {
            base.OnScroll(eventData);
            if (PropagateOnScroll)
                Propagate(eventData, ExecuteEvents.scrollHandler);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            if (PropagateOnSelect)
                Propagate(eventData, ExecuteEvents.selectHandler);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            base.OnSubmit(eventData);
            if (PropagateOnSubmit)
                Propagate(eventData, ExecuteEvents.submitHandler);
        }

        public override void OnUpdateSelected(BaseEventData eventData)
        {
            base.OnUpdateSelected(eventData);
            if (PropagateOnUpdateSelected)
                Propagate(eventData, ExecuteEvents.updateSelectedHandler);
        }

        private void Propagate<T>(BaseEventData eventData, ExecuteEvents.EventFunction<T> callback) where T : IEventSystemHandler
        {
            ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, callback);
        }
    }
}