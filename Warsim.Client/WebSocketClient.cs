using System;

using Newtonsoft.Json;

using Warsim.Events;
using Warsim.Events.Messages;

using WebSocketSharp;

namespace Warsim.Client
{
    public class WebSocketClient
    {
        private readonly EventProcessor _eventProcessor;

        public WebSocket WebSocket { get; }

        public Action<object, CloseEventArgs> OnCloseHandler { get; set; }

        public WebSocketClient(string url)
        {
            this.WebSocket = new WebSocket(url);

            this.WebSocket.OnMessage += this.OnMessage;
            this.WebSocket.OnClose += this.OnClose;

            this.OnCloseHandler = delegate { };

            this._eventProcessor = new EventProcessor();
        }

        private void OnClose(object sender, CloseEventArgs e)
        {
            this.OnCloseHandler(sender, e);
        }

        public void RegisterHandler<T>(Action<T, DateTime> handler) where T : IEventMessage
        {
            this._eventProcessor.RegisterHandler(new Events.EventHandler<T>(handler));
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
    
            if (e.IsText)
            {
                var ev = JsonConvert.DeserializeObject<Event>(e.Data);
          

                this._eventProcessor.Process(ev);
                Console.WriteLine($"OnMessage: {ev.Type}");
            }

        }

        public void Connect()
        {
            this.WebSocket.Connect();
        }

        public void Send(string data)
        {
            this.WebSocket.Send(data);
        }

        public void Close()
        {
            this.WebSocket.Close();
        }
    }
}