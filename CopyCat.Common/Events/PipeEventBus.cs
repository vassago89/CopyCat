using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CopyCat.Common.Events
{
    class PipeEventBus<T> : EventBus<T> where T : IEvent
    {
        private BinaryFormatter _formatter = new BinaryFormatter();
        private Task _serverTask;

        private NamedPipeClientStream _client;

        public override void Publish(IEvent @event)
        {
            var _formatter = new BinaryFormatter();

            if (_client == null || _client.IsConnected == false)
            {
                _client = new NamedPipeClientStream(".", typeof(T).Name, PipeDirection.InOut);
                _client.Connect();
            }

            _formatter.Serialize(_client, @event);
        }

        public override void Subscribe(Action<T> action)
        {
            base.Subscribe(action);


            _serverTask = Task.Run(() =>
            {
                try
                {
                    var server = new NamedPipeServerStream(
                            typeof(T).Name,
                            PipeDirection.InOut);

                    server.WaitForConnection();

                    while (true)
                    {
                        server.WaitForPipeDrain();
                        var @event = (T)_formatter.Deserialize(server);
                        base.Publish(@event);
                        server.Flush();
                    }
                }
                catch (Exception e)
                {

                }
            });
        }
    }
}
