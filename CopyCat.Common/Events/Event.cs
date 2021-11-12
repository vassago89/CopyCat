using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CopyCat.Common.Events
{
    public interface IEvent
    {

    }

    [Serializable]
    public class @Event<TData> : IEvent where TData : class, ISerializable
    {
        public TData Data { get; }

        public @Event(TData data)
        {
            Data = data;
        }
    }
}
