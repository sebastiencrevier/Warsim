using System;

namespace Warsim.Core.Chat
{
    public class Message
    {
        public DateTime Timestamp { get; set; }

        public string UserId { get; set; }

        public string Content { get; set; }
    }
}