using System;
using System.Collections.Generic;
using System.Text;

namespace CopyCat.Common.Events
{
    public interface IEventBus
    {
        void Publish(IEvent @event);
    }

    public class EventBus<T> : IEventBus where T : IEvent
    {
        protected event Action<T> Actions;

        public virtual void Publish(IEvent @event)
        {
            Actions?.Invoke((T)@event);
        }

        public virtual void Subscribe(Action<T> action)
        {
            Actions += action;
        }
    }
}
