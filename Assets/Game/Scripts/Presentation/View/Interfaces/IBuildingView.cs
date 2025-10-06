using UnityEngine;

namespace Game.Scripts.Presentation.View.Interfaces
{
    public interface IBuildingView
    {
        void SetHighlight(bool active, Color color = default);
        void Destroy();
    }
}