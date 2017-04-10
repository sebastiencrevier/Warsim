using System;
using System.Collections.Generic;

using Warsim.Core.Game.Entities;

namespace Warsim.Events.Messages.Game
{
    public class GameStateMessage : IEventMessage
    {
        public Guid GameId { get; set; }

        public IList<Node> SceneObjects { get; set; }
    }
}