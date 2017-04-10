using System;

using Warsim.Core.Game;

namespace Warsim.Events.Messages.Game
{
    public class GameModeSwitchMessage : IEventMessage
    {
        public GameMode Mode { get; set; }
    }
}