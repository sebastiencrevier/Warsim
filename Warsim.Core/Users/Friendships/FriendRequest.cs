using System;

namespace Warsim.Core.Users.Friendships
{
    public class FriendRequest
    {
        public virtual string UserId { get; set; }

        public virtual string FutureFriendId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual ApplicationUser FutureFriend { get; set; }

        public DateTime? RequestTime { get; set; }

        public bool Accepted { get; set; }
    }
}