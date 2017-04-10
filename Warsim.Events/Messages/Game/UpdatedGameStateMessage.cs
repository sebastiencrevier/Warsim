using System;
using System.Collections.Generic;

using Warsim.Core.Game.Entities;

namespace Warsim.Events.Messages.Game
{
    public class UpdatedGameStateMessage : IEventMessage
    {
        public Guid GameId { get; set; }

        public string UserToken { get; set; }

        public IList<Node> UpdatedSceneObjects { get; set; }
    }
}