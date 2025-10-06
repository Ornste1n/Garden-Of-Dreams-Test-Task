using System;
using UnityEngine;
using System.Collections.Generic;

namespace Game.Scripts.Presentation.View.Interfaces
{
    public interface IBuildingPanelView
    {
        void ShowBuildings(IReadOnlyDictionary<string, Sprite> buildings);
        void SetActive(bool active);
        void ResetSelected();
        
        event Action<string> OnPlaceClicked;
        event Action OnCanceled;
        event Action OnDeleteClicked;
    }
}