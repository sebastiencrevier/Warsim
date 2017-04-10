using System;

namespace Warsim.Events.Messages.Game
{
    public class GameInviteMessage : IEventMessage
    {
        public Guid GameId { get; set; }

        public string Title { get; set; }

        public string UserId { get; set; }

        public string Username { get; set; }

        public Guid NotificationId { get; set; }
    }
}