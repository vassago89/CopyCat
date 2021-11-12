using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;

namespace CopyCat.Common.Events
{
    enum EEventBusType
    {
        Local,
        Pipe
    }

    public class EventBusMediator
    {
        private EEventBusType _eventBusType;
        private Dictionary<Type, IEventBus> _busDictionary;

        public EventBusMediator()
        {
            _eventBusType = EEventBusType.Pipe;
            _busDictionary = new Dictionary<Type, IEventBus>();
        }

        public void Publish(IEvent @event)
        {
            var bus = AddOrReturn(@event.GetType());
            bus.Publish(@event);
        }

        public void Subscribe<T>(Action<T> action) where T : IEvent
        {
            var bus = AddOrReturn(typeof(T));

            switch (_eventBusType)
            {
                case EEventBusType.Local:
                    ((EventBus<T>)bus).Subscribe(action);
                    break;
                case EEventBusType.Pipe:
                    ((PipeEventBus<T>)bus).Subscribe(action);
                    break;
            }
        }

        private IEventBus AddOrReturn(Type eventType)
        {
            if (_busDictionary.ContainsKey(eventType) == false)
            {
                Type @type = null;
                switch (_eventBusType)
                {
                    case EEventBusType.Local:
                        @type = typeof(EventBus<>);
                        break;
                    case EEventBusType.Pipe:
                        @type = typeof(PipeEventBus<>);
                        break;
                }

                var constructType = @type.MakeGenericType(new Type[] { eventType });
                _busDictionary[eventType] = (IEventBus)Activator.CreateInstance(constructType);
            }

            return _busDictionary[eventType];
        }
    }
}
