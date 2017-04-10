using System;
using System.Collections.Generic;

using Warsim.Core.Game.Entities;

namespace Warsim.Events.Messages.Game
{
    public class LocalGameStateMessage : IEventMessage
    {
        public IList<Node> SceneObjects { get; set; }
    }
}