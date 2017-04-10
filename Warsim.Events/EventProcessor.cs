using System;
using System.Collections.Generic;

using Warsim.Events.Messages;

namespace Warsim.Events
{
    public class EventProcessor
    {
        public IDictionary<string, IList<IEventHandler>> Handlers { get; set; } = new Dictionary<string, IList<IEventHandler>>();

        public void RegisterHandler<T>(EventHandler<T> handler) where T : IEventMessage
        {
            var eventType = typeof(T).Name;

            if (this.Handlers.ContainsKey(eventType))
            {
                this.Handlers[eventType].Add(handler);
            }
            else
            {
                this.Handlers.Add(eventType, new List<IEventHandler> { handler });
            }
        }

        public bool Process(Event ev)
        {
            if (!this.Handlers.ContainsKey(ev.Type))
            {
                return false;
            }

            var handled = false;

            foreach (var handlers in this.Handlers[ev.Type])
            {
                if (handlers.Handle(ev.Message, ev.Timestamp))
                {
                    handled = true;
                }
            }

            return handled;
        }
    }
}