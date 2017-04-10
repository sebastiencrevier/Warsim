using System;
using System.Collections.Generic;

using Warsim.Core.Game.Entities;

namespace Warsim.API.ApiControllers.Messages
{
    public class SyncMapMessage
    {
        public Guid MapId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Password { get; set; }

        public IList<Node> SceneObjects { get; set; }
    }

    public class UpdateExistingMapMessage
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public Guid MapId { get; set; }

        public string Password { get; set; }
    }
}