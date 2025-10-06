using UnityEngine;
using Game.Scripts.Presentation.View.Interfaces;

namespace Game.Scripts.Presentation.View
{
    public class BuildingPreviewView : MonoBehaviour, IBuildingPreviewView
    {
        [SerializeField] private float _zOffset = -0.02f;
        [SerializeField] private bool _showGhostColor = true;
        [Space]
        [SerializeField] private Color _ghostColor = new Color(1f, 1f, 1f, 0.6f);
        [SerializeField] private Color _errorColor = new Color(1f, 1f, 1f, 0.6f);

        private SpriteRenderer _spriteRenderer;
        private Color _origColor;
        
        public bool IsVisible { get; private set; }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _origColor = _spriteRenderer.color;
            Hide();
        }

        public void Show(Sprite sprite)
        {
            if (sprite == null) return;
            _spriteRenderer.sprite = sprite;
            _spriteRenderer.enabled = true;
            if (_showGhostColor)
            {
                Color c = _ghostColor;
                _spriteRenderer.color = c;
            }

            IsVisible = true;
        }

        public void Hide()
        {
            _spriteRenderer.enabled = false;
            _spriteRenderer.sprite = null;
            _spriteRenderer.color = _origColor;
            IsVisible = false;
        }

        public void SetPosition(Vector3 worldPos, bool isFreePlace)
        {
            transform.position = new Vector3(worldPos.x, worldPos.y, worldPos.z + _zOffset);
            _spriteRenderer.color = isFreePlace ? _ghostColor : _errorColor;
        }

        public void SetAlpha(float alpha)
        {
            if (_spriteRenderer.sprite == null) return;
            Color c = _spriteRenderer.color;
            c.a = Mathf.Clamp01(alpha);
            _spriteRenderer.color = c;
        }
    }
}