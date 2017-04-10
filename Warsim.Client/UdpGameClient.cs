using System;
using System.Net.Sockets;
using System.Threading.Tasks;

using Warsim.Events;
using Warsim.Events.Messages;
using Warsim.Events.Messages.Game;

namespace Warsim.Client
{
    public class UdpGameClient
    {
        public UdpClient Client { get; }

        private readonly EventProcessor _eventProcessor;

        private bool _isListening;

        public UdpGameClient(string hostname, int port)
        {
            this.Client = new UdpClient();
            this.Client.Connect(hostname, port);

            this._eventProcessor = new EventProcessor();
        }

        public void StartListening()
        {
            this._isListening = true;

            Task.Run(async () =>
            {
                while (this._isListening)
                {
                    try
                    {
                        await this.ReceiveAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            });
        }

        public void StopListening()
        {
            this._isListening = false;  
        }

        public void RegisterOnGameStateChangeListener(Action<GameStateMessage, DateTime> listener)
        {
            this._eventProcessor.RegisterHandler(new Events.EventHandler<GameStateMessage>(listener));
        }

        public void Send(IEventMessage message)
        {
            var eventBuilder = EventBuilder.Build(message);
            var data = eventBuilder.ToByteArray();

            this.Client.Send(data, data.Length);
        }

        public async Task ReceiveAsync()
        {
            var received = await this.Client.ReceiveAsync();

            var eventBuilder = EventBuilder.Build(received.Buffer);

            this._eventProcessor.Process(eventBuilder.Event);
        }
    }
}