using System;
using System.Collections.Generic;

using Warsim.Core.Dtos;

namespace Warsim.API.ApiControllers.Messages
{
    public class UpdateProfileMessage
    {
        public string Description { get; set; }

        public string AvatarInBase64 { get; set; }
    }

    public class UserProfileMessage
    {
        public string Username { get; set; }

        public string Description { get; set; }

        public bool IsConnected { get; set; }

        public bool IsPlaying { get; set; }

        public int GameCreatedCount { get; set; }

        public int GameJoinedCount { get; set; }

        public int MapModifiedCount { get; set; }

        public int PostAddedCount { get; set; }

        public int WallAddedCount { get; set; }

        public int LineAddedCount { get; set; }
    }

    public class UsersResponse
    {
        public IList<WarsimClientUser> AllUsers { get; set; }

        public IList<string> FriendIds { get; set; }
    }
}