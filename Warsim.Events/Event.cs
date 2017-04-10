using System;

namespace Warsim.Events
{
    public class Event
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public string Message { get; set; }

        public DateTime Timestamp { get; set; }
    }
}