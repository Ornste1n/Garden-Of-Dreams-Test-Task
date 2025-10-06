using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Game.Scripts.Presentation.View.Interfaces;

namespace Game.Scripts.Presentation.View
{
    public class BuildingPanelView : MonoBehaviour, IBuildingPanelView
    {
        [SerializeField] private Transform _contentParent;
        [SerializeField] private BuildingItemView _buildingItemPrefab;
        [Space] 
        [SerializeField] private Button _placeButton;
        [SerializeField] private Button _deleteButton;

        private BuildingItemView _selectedItem;

        public event Action<string> OnPlaceClicked;
        public event Action OnCanceled;
        public event Action OnDeleteClicked;

        private void Awake()
        {
            _placeButton.onClick.AddListener(HandlePlaceButton);
            _deleteButton.onClick.AddListener(HandleDeleteButton);
        }

        public void ShowBuildings(IReadOnlyDictionary<string, Sprite> buildings)
        {
            foreach ((string id, Sprite sprite) in buildings)
            {
                BuildingItemView itemView = Instantiate(_buildingItemPrefab, _contentParent);
                itemView.Initialize(id, sprite, HandleSelectEvent);
            }
            
            _contentParent.gameObject.SetActive(false);
        }

        private void HandleSelectEvent(BuildingItemView itemView)
        {
            if(_selectedItem != null)
                _selectedItem.SetOutline(false);

            itemView.SetOutline(true);
            _selectedItem = itemView;
            
            OnPlaceClicked?.Invoke(_selectedItem.Id);
        }

        private void HandlePlaceButton()
        {
            bool contentState = _contentParent.gameObject.activeInHierarchy;

            if (contentState)
            {
                if (_selectedItem != null)
                {
                    _selectedItem.SetOutline(false);
                    _selectedItem = null;
                }
                OnCanceled?.Invoke();
            }
            
            _contentParent.gameObject.SetActive(!contentState);
        }

        public void SetActive(bool active)
        {
            if (!active && _selectedItem != null)
            {
                _selectedItem.SetOutline(false);
                _selectedItem = null;
            }

            _contentParent.gameObject.SetActive(active);
        }

        private void HandleDeleteButton() => OnDeleteClicked?.Invoke();

        private void OnDestroy()
        {
            _placeButton.onClick.RemoveListener(HandlePlaceButton);
            _deleteButton.onClick.RemoveListener(HandleDeleteButton);
        }
    }
}