#nullable enable

using UnityEngine;
using UnityEngine.EventSystems;

namespace ToggleableBindings.UI
{
    /// <summary>
    /// Sets the object this behaviour is placed on as the current EventSystem's selected object,
    /// optionally re-selecting the originally-selected object when this component is destroyed.
    /// </summary>
    public class SelectedTemporarily : MonoBehaviour
    {
        [SerializeField]
        private bool _reselectLast = true;
        private GameObject? _lastSelected;

        /// <summary>
        /// Gets or sets whether to reselect the originally-selected 
        /// object when this component is destroyed.
        /// <br/>
        /// Default: <see langword="true"/>
        /// </summary>
        public bool ReselectLast { get => _reselectLast; set => _reselectLast = value; }

        private void Awake()
        {
            _lastSelected = EventSystem.current.currentSelectedGameObject;
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        private void OnDestroy()
        {
            if (_reselectLast)
                EventSystem.current.SetSelectedGameObject(_lastSelected);
        }
    }
}