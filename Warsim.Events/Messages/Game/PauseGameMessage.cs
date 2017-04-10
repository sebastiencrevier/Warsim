using System;

namespace Warsim.Events.Messages.Game
{
    public class PauseGameMessage : IEventMessage
    {
        public bool IsPaused { get; set; }
    }
}