using System;
using UnityEngine;
using System.Collections.Generic;

namespace Game.Scripts.Usecases.Application.Interfaces
{
    public interface IBuildingSpritesConfig  : IDisposable
    {
        IReadOnlyDictionary<string, Sprite> Sprites { get; }
        Sprite GetById(string guid);
    }
}