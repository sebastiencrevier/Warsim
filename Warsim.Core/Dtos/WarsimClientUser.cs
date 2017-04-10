using System;

using Warsim.Core.Users;

namespace Warsim.Core.Dtos
{
    public class WarsimClientUser
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public bool IsConnected { get; set; }

        public Guid GameId { get; set; }

        public bool IsPlaying => !this.GameId.Equals(Guid.Empty);

        public static WarsimClientUser Map(WarsimUser user)
        {
            return new WarsimClientUser
            {
                Id = user.UserId,
                Username = user.Username,
                GameId = user.ActiveGameId,
                IsConnected = true
            };
        }

        public static WarsimClientUser Map(ApplicationUser user)
        {
            return new WarsimClientUser
            {
                Id = user.Id,
                Username = user.Username,
                IsConnected = false
            };
        }
    }
}