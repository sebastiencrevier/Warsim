using System;

namespace Warsim.Events.Messages.Chat
{
    public class UserLeftChannelMessage : IEventMessage
    {
        public string UserId { get; set; }

        public string Username { get; set; }
    }
}