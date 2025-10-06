using UnityEngine;
using Game.Scripts.Presentation.View.Interfaces;

namespace Game.Scripts.Presentation.View
{
    public class BuildingView : MonoBehaviour, IBuildingView
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private Color _defaultColor;

        private void Awake()
        {
            _defaultColor = _spriteRenderer.color;
        }

        public void SetHighlight(bool active, Color color = default)
        {
            if(_spriteRenderer == null) return;
            
            _spriteRenderer.color = active ? color : _defaultColor;
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}