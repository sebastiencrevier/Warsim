using System;
using System.Collections.Generic;

using Warsim.Core.Game.Entities;

namespace Warsim.API.ApiControllers.Messages
{
    public class CreateGameMessage
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Password { get; set; }
    }

    public class CreateGameWithNewMapMessage
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Password { get; set; }

        public IList<Node> SceneObjects { get; set; }
    }

    public class CreateGameWithExistingMapMessage
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public Guid MapId { get; set; }

        public string ExistingPassword { get; set; }

        public string Password { get; set; }
    }

    public class SendGameInviteMessage
    {
        public string InviteeId { get; set; }
    }
}