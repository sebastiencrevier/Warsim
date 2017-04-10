using System;

using Warsim.Core.Users;

namespace Warsim.Core.Chat.Channels
{
    public class Conversation : Channel
    {
        public WarsimUser User1 { get; set; }

        public WarsimUser User2 { get; set; }

        public Conversation(WarsimUser user1, WarsimUser user2)
            : base(ChannelType.PrivateConversation, 2)
        {
            this.User1 = user1;
            this.User2 = user2;
        }
    }
}