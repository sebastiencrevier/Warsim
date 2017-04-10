using System;

namespace Warsim.Core.Users.Friendships
{
    public static class FriendshipsExtensions
    {
        public static FriendRequest SendFriendRequest(this ApplicationUser user, ApplicationUser futureFriend)
        {
            var request = new FriendRequest
            {
                FutureFriend = futureFriend,
                User = user,
                RequestTime = DateTime.Now,
                Accepted = false
            };

            user.FriendRequests.Add(request);

            return request;
        }

        public static Friend AcceptFriendRequest(this ApplicationUser user, FriendRequest friendRequest)
        {
            // Wrong friend request
            if (friendRequest.Accepted || friendRequest.UserId == user.Id || friendRequest.FutureFriendId != user.Id)
            {
                return null;
            }

            friendRequest.Accepted = true;

            var friend = new Friend
            {
                User1 = friendRequest.User,
                User2 = user,
                BecameFriendsTime = DateTime.Now
            };

            user.Friends.Add(friend);

            return friend;
        }
    }
}