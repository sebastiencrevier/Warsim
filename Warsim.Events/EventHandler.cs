using System;
using System.Diagnostics;

using Newtonsoft.Json;

using Warsim.Events.Messages;

namespace Warsim.Events
{
    public class EventHandler<T> : IEventHandler
        where T : IEventMessage
    {
        private readonly Action<T, DateTime> _handler;

        public EventHandler(Action<T, DateTime> handler)
        {
            this._handler = handler;
        }

        public bool Handle(string message, DateTime timestamp)
        {
            try
            {
                this._handler(JsonConvert.DeserializeObject<T>(message), timestamp);
                return true;
            }
            catch
            {
                Trace.WriteLine($"Cannot deserialize message to object type {typeof(T)}");
                return false;
            }
        }
    }
}