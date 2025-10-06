using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Presentation.View
{
    public class BuildingItemView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _outline;
        [SerializeField] private Button _button;

        public string Id { get; private set; }
        private Action<BuildingItemView> _clickAction;
        
        public void Initialize(string id, Sprite sprite, Action<BuildingItemView> callback)
        {
            Id = id;
            _icon.sprite = sprite;
            _clickAction = callback;
            _button.onClick.AddListener(HandleClick);
        }

        private void HandleClick() => _clickAction?.Invoke(this);

        public void SetOutline(bool active)
        {
            _outline.gameObject.SetActive(active);
        }
        
        private void OnDestroy()
        {
            _button.onClick.RemoveListener(HandleClick);
        }
    }
}