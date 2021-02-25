#nullable enable

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ToggleableBindings.UI
{
    internal class BindingApplyMsg : MonoBehaviour, ISubmitHandler, IPointerClickHandler, ICancelHandler, IMoveHandler
    {
        private bool _ableToClose;

        private void Start()
        {
            gameObject.transform.position = new Vector3(0f, 2.77f, -20f); // Dunno why, but it's in the FSM and seems to work. May work without but w/e.
            PlayMakerFSM.BroadcastEvent("CHARM MSG");
            FSMUtility.SendEventToGameObject(gameObject, "UP", true);

            StartCoroutine(WaitForTimeout());
        }

        private IEnumerator WaitForTimeout()
        {
            yield return new WaitForSeconds(0.25f);
            _ableToClose = true;
            yield return new WaitForSeconds(4.5f);
            Close();
        }

        private void Close()
        {
            if (_ableToClose)
            {
                _ableToClose = false;
                PlayMakerFSM.BroadcastEvent("CHARM MSG DOWN");
                FSMUtility.SendEventToGameObject(gameObject, "DOWN", true);

                Destroy(gameObject, 0.25f);
            }
        }

        private void DoClose(BaseEventData eventData)
        {
            eventData.Use();
            Close();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            DoClose(eventData);
        }

        public void OnCancel(BaseEventData eventData)
        {
            DoClose(eventData);
        }

        public void OnMove(AxisEventData eventData)
        {
            DoClose(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSubmit(eventData);
        }
    }
}