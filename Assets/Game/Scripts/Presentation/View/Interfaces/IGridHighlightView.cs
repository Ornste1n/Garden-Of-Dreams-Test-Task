using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Scripts.Presentation.View.Interfaces
{
    public interface IGridHighlightView
    {
        void HighlightVisible(Vector3 bottomLeft, Vector2 cellSize, Vector2 maskSize, Texture2D maskTexture);

        void SetValidity(bool valid);
        void ApplyMask();
    }
}