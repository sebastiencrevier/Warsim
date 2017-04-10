using System;

namespace Warsim.Events.Messages.Game
{
    public class GamePrivacyChangeMessage : IEventMessage
    {
        public string Password { get; set; }
    }
}