using System;
using System.Collections.Generic;

using Warsim.Core.Chat;
using Warsim.Core.Dtos;

namespace Warsim.Events.Messages.Chat
{
    public class ChannelStateMessage : IEventMessage
    {
        public IList<Message> Messages { get; set; }

        public IList<WarsimClientUser> ActiveUsers { get; set; }
    }
}