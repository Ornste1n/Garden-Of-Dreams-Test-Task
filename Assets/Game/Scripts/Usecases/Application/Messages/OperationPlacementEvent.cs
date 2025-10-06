using Game.Scripts.Domain.Game;
using UnityEngine;

namespace Game.Scripts.Usecases.Application.Messages
{
    public struct ConfirmPlacementEvent
    {
        public Vector3Int CellPosition { get; }
        
        public ConfirmPlacementEvent(Vector3Int cellPosition)
        {
            CellPosition = cellPosition;
        }
    }
    
    public struct CanceledPlacementEvent {}
    public struct DeletePlacementModeEvent { }
    
    public struct ChoicePlacementEvent
    {
        public string Guid { get; }
        
        public ChoicePlacementEvent(string guid)
        {
            Guid = guid;
        }
    }
    
    public struct OperationPlacementEvent
    {
        public enum OperationState
        {
            Place,
            Delete
        }

        public Occupancy Occupancy { get; }
        public OperationState State { get; }

        public OperationPlacementEvent(Occupancy result, OperationState state)
        {
            State = state;
            Occupancy = result;
        }
    }
}