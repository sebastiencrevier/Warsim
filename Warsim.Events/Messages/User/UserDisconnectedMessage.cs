using System;

namespace Warsim.Events.Messages.User
{
    public class UserDisconnectedMessage : IEventMessage
    {
        public string UserId { get; set; }
    }
}