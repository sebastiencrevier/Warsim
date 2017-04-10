using System;

namespace Warsim.API.ApiControllers.Messages
{
    public class NewFriendRequestMessage
    {
        public string FutureFriendId { get; set; }
    }

    public class AcceptFriendRequestMessage
    {
        public string FutureFriendId { get; set; }
    }
}