using System;
using System.Collections.Generic;

using Warsim.Core.Notifications;
using Warsim.Core.Users.Friendships;

namespace Warsim.Core.Users
{
    public class ApplicationUser
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Description { get; set; }

        public string AppleDeviceToken { get; set; }

        public int GameCreatedCount { get; set; }

        public int GameJoinedCount { get; set; }

        public int MapModifiedCount { get; set; }

        public int PostAddedCount { get; set; }

        public int WallAddedCount { get; set; }

        public int LineAddedCount { get; set; }

        public virtual ICollection<Friend> Friends { get; set; }

        public virtual ICollection<FriendRequest> FriendRequests { get; set; }

        public virtual ICollection<ApplicationUserLogin> UserLogins { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; }
    }
}