using System;

using Warsim.Core.Users;

namespace Warsim.Core.Chat.Channels
{
    public class PublicChannel : Channel
    {
        public string Name { get; set; }

        public WarsimUser Owner { get; set; }

        public PublicChannel(string name, WarsimUser owner)
            : base(ChannelType.PublicChannel, 20)
        {
            this.Name = name;
            this.Owner = owner;
        }
    }
}