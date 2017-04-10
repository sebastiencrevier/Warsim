using System;
using System.Collections.Generic;
using System.Linq;

using Warsim.Core.Game;

namespace Warsim.Core.Dtos
{
    public class WarsimClientGame
    {
        public Guid Id { get; set; }

        public Guid MapId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public IList<WarsimClientUser> Players { get; set; }

        public IList<WarsimClientUser> Spectators { get; set; }

        public GameMode Mode { get; set; }

        public string OwnerId { get; set; }

        public bool IsPrivate { get; set; }

        public static WarsimClientGame Map(GameHost gameHost)
        {
            return new WarsimClientGame
            {
                Id = gameHost.Id,
                MapId = gameHost.Map.Id,
                Title = gameHost.Map.Title,
                Description = gameHost.Map.Description,
                Players = gameHost.Players.Values.Select(WarsimClientUser.Map).ToList(),
                Spectators = gameHost.Spectators.Values.Select(WarsimClientUser.Map).ToList(),
                Mode = gameHost.Mode,
                OwnerId = gameHost.OwnerId,
                IsPrivate = gameHost.Map.IsPrivate
            };
        }
    }
}