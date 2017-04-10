using System;
using System.Text;

using Newtonsoft.Json;

using Warsim.Events.Messages;

namespace Warsim.Events
{
    public class EventBuilder
    {
        public Event Event { get; private set; }

        private EventBuilder()
        {
        }

        public static EventBuilder Build<T>(T eventMessage) where T : class, IEventMessage
        {
            var eventBuilder = new EventBuilder();

            eventBuilder.Event = new Event
            {
                Id = Guid.NewGuid().ToString(),
                Message = JsonConvert.SerializeObject(eventMessage),
                Type = eventMessage.GetType().Name,
                Timestamp = DateTime.Now
            };

            return eventBuilder;
        }

        public static EventBuilder Build(string data)
        {
            var eventBuilder = new EventBuilder();

            eventBuilder.Event = JsonConvert.DeserializeObject<Event>(data);
            eventBuilder.Event.Timestamp = DateTime.Now;

            return eventBuilder;
        }

        public static EventBuilder Build(byte[] data)
        {
            return EventBuilder.Build(Encoding.ASCII.GetString(data, 0, data.Length));
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this.Event);
        }

        public byte[] ToByteArray()
        {
            return Encoding.ASCII.GetBytes(this.Serialize());
        }
    }
}