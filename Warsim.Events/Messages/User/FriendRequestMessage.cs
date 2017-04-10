using System;

namespace Warsim.Events.Messages.User
{
    public class FriendRequestMessage : IEventMessage
    {
        public string UserId { get; set; }

        public string Username { get; set; }

        public Guid NotificationId { get; set; }
    }
}