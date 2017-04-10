using System;

namespace Warsim.Events.Messages.Game
{
    public class PlayerConnectedMessage : IEventMessage
    {
        public string UserId { get; set; }

        public string Username { get; set; }
    }
}