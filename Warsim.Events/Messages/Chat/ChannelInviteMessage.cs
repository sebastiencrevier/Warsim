using System;

namespace Warsim.Events.Messages.Chat
{
    public class ChannelInviteMessage : IEventMessage
    {
        public string UserId { get; set; }

        public string Username { get; set; }

        public Guid ChannelId { get; set; }

        public string ChannelName { get; set; }

        public Guid NotificationId { get; set; }
    }
}