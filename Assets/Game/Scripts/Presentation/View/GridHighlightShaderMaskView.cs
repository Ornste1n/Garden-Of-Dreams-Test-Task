using UnityEngine;
using Game.Scripts.Presentation.View.Interfaces;

namespace Game.Scripts.Presentation.View
{
    [RequireComponent(typeof(MeshRenderer))]
    public class GridHighlightShaderMaskView : MonoBehaviour, IGridHighlightView
    {
        [SerializeField] private Material _materialInstance;

        #region Shader Properties
        private static readonly int s_origin = Shader.PropertyToID("_Origin");
        private static readonly int s_cellSize = Shader.PropertyToID("_CellSize");
        private static readonly int s_maskBounds = Shader.PropertyToID("_MaskBounds");
        private static readonly int s_isValid = Shader.PropertyToID("_IsValid");
        private static readonly int s_maskTex = Shader.PropertyToID("_MaskTex");
        #endregion
        
        private MeshRenderer _renderer;
        private MaterialPropertyBlock _mpb;
        private Texture2D _maskTexture;

        private bool _isValid = true;
        private Vector3 _bottomLeft;
        private Vector2 _cellSize;

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            _materialInstance = _renderer.sharedMaterial;
            _mpb = new MaterialPropertyBlock();
            
            AlignQuadToMask(0, 0);
        }

        public void HighlightVisible(Vector3 bottomLeft, Vector2 cellSize, Vector2 maskSize, Texture2D maskTexture)
        {
            _bottomLeft = bottomLeft;
            _cellSize = cellSize;
            _maskTexture = maskTexture;
            
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetVector(s_origin, new Vector4(_bottomLeft.x, _bottomLeft.y, 0f, 0f));
            _mpb.SetVector(s_cellSize, new Vector4(_cellSize.x, _cellSize.y, 0f, 0f));
            _mpb.SetVector(s_maskBounds, new Vector4(0, 0, maskSize.x, maskSize.y));
            _mpb.SetFloat(s_isValid, _isValid ? 1f : 0f);
            _mpb.SetTexture(s_maskTex, _maskTexture);
            _renderer.SetPropertyBlock(_mpb);

            AlignQuadToMask(maskSize.x, maskSize.y);
            ApplyMask();
        }

        public void SetValidity(bool valid)
        {
            _isValid = valid;
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(s_isValid, _isValid ? 1f : 0f);
            _renderer.SetPropertyBlock(_mpb);
        }

        public void ApplyMask()
        {
            if(_maskTexture == null) return;
            
            _maskTexture.Apply(false, false);
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetTexture(s_maskTex, _maskTexture);
            _renderer.SetPropertyBlock(_mpb);
        }

        private void AlignQuadToMask(float maskWidth, float maskHeight)
        {
            transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            transform.position = _bottomLeft + new Vector3(maskWidth * _cellSize.x * 0.5f, maskHeight * _cellSize.y * 0.5f, 0);
            transform.localScale = new Vector3((maskWidth * _cellSize.x) / 10f, 1f, (maskHeight * _cellSize.y) / 10f);
        }
    }
}
