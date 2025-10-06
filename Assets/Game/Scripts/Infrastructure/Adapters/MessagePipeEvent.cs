using System;
using MessagePipe;
using Game.Scripts.Usecases.Application.Interfaces;

namespace Game.Scripts.Infrastructure.Adapters
{
    public class MessagePipeEvent<TIn, TOut> : IEvent<TOut>
    {
        private readonly ISubscriber<TIn> _subscriber;
        private readonly Func<TIn, (bool matched, TOut value)> _map;

        public MessagePipeEvent(ISubscriber<TIn> subscriber, Func<TIn, (bool, TOut)> map)
        {
            _subscriber = subscriber;
            _map = map;
        }

        public IDisposable Subscribe(Action<TOut> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            return _subscriber.Subscribe(inValue =>
            {
                (bool ok, TOut val) = _map(inValue);
                if (ok) handler(val);
            });
        }
    }
}