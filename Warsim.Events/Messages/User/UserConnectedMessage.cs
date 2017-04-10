using System;

namespace Warsim.Events.Messages.User
{
    public class UserConnectedMessage : IEventMessage
    {
        public string UserId { get; set; }

        public string Username { get; set; }
    }
}