using System;
using System.Collections.Generic;

using Warsim.Core.Dtos;

namespace Warsim.API.ApiControllers.Messages
{
    public class ChatChannelsResponseMessage
    {
        public IList<WarsimClientChannel> Channels { get; set; }

        public IList<WarsimClientConversation> Conversations { get; set; }
    }

    public class ChatCreateConversationMessage
    {
        public string SecondUserId { get; set; }
    }

    public class ChatCreatePublicChannelMessage
    {
        public string ChannelName { get; set; }

        public IList<string> ParticipantsIds { get; set; }
    }
}