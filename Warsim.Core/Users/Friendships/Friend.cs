using System;

namespace Warsim.Core.Users.Friendships
{
    public class Friend
    {
        public virtual string User1Id { get; set; }

        public virtual string User2Id { get; set; }

        public virtual ApplicationUser User1 { get; set; }

        public virtual ApplicationUser User2 { get; set; }

        public DateTime? BecameFriendsTime { get; set; }
    }
}