using System;

namespace Warsim.Events.Messages.Chat
{
    public class UserJoinedChannelMessage : IEventMessage
    {
        public string UserId { get; set; }

        public string Username { get; set; }
    }
}