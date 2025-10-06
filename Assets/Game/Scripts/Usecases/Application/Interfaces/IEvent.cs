using System;

namespace Game.Scripts.Usecases.Application.Interfaces
{
    public interface IEvent<T>
    {
        IDisposable Subscribe(Action<T> handler);
    }
}