using System;

namespace Warsim.Events.Messages.Chat
{
    public class ChatMessageMessage : IEventMessage
    {
        public string UserId { get; set; }

        public string Username { get; set; }

        public string Content { get; set; }
    }
}