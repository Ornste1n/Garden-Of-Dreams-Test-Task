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

        private SpriteRenderer _sr;
        private Color _origColor;
        
        public bool IsVisible { get; private set; }

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _origColor = _sr.color;
            Hide();
        }

        public void Show(Sprite sprite)
        {
            if (sprite == null) return;
            _sr.sprite = sprite;
            _sr.enabled = true;
            if (_showGhostColor)
            {
                Color c = _ghostColor;
                _sr.color = c;
            }

            IsVisible = true;
        }

        public void Hide()
        {
            _sr.enabled = false;
            _sr.sprite = null;
            _sr.color = _origColor;
            IsVisible = false;
        }

        public void SetPosition(Vector3 worldPos, bool isFreePlace)
        {
            transform.position = new Vector3(worldPos.x, worldPos.y, worldPos.z + _zOffset);
            _sr.color = isFreePlace ? _ghostColor : _errorColor;
        }

        public void SetAlpha(float alpha)
        {
            if (_sr.sprite == null) return;
            Color c = _sr.color;
            c.a = Mathf.Clamp01(alpha);
            _sr.color = c;
        }
    }
}