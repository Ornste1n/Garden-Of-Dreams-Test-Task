using UnityEngine;

namespace Game.Scripts.Presentation.View.Interfaces
{
    public interface IBuildingPreviewView
    {
        void Show(Sprite sprite);
        void SetPosition(Vector3 worldPos, bool isFreePlace);
        
        void Hide();

        void SetAlpha(float alpha);

        bool IsVisible { get; }
    }
}