using System;

namespace Warsim.Events.Messages.Game
{
    public class PlayerDisconnectedMessage : IEventMessage
    {
        public string UserId { get; set; }

        public string Username { get; set; }
    }
}