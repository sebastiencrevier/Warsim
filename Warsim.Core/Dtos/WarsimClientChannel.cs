using System;
using System.Collections.Generic;
using System.Linq;

using Warsim.Core.Chat.Channels;

namespace Warsim.Core.Dtos
{
    public class WarsimClientChannel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string OwnerId { get; set; }

        public IList<WarsimClientUser> ActiveUsers { get; set; }

        public static WarsimClientChannel Map(PublicChannel channel)
        {
            return new WarsimClientChannel
            {
                Id = channel.Id,
                Name = channel.Name,
                OwnerId = channel.Owner?.UserId,
                ActiveUsers = channel.ActiveUsers.Select(WarsimClientUser.Map).ToList()
            };
        }
    }
}