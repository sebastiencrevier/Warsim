using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Warsim.Core.Helpers.Http;
using Warsim.Core.Users;
using Warsim.Core.Users.Friendships;

namespace Warsim.Core.DAL
{
    public class FriendshipRepository : ApplicationRepository
    {
        public FriendshipRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }

        public FriendRequest CreateFriendRequest(string userId, string futureFriendId)
        {
            if (userId == futureFriendId)
            {
                throw HttpResponseExceptionHelper.Create("Cannot send a friend request to yourself", HttpStatusCode.BadRequest);
            }

            var existingFriendRequest = this.GetFriendRequest(userId, futureFriendId);

            if (existingFriendRequest != null)
            {
                if (existingFriendRequest.Accepted)
                {
                    throw HttpResponseExceptionHelper.Create("You are already friends", HttpStatusCode.BadRequest);
                }

                throw HttpResponseExceptionHelper.Create("This friend request already exists", HttpStatusCode.BadRequest);
            }

            var userRepo = new UserRepository(this.DbContext);

            var user = userRepo.GetUserById(userId);
            var futureFriend = userRepo.GetUserById(futureFriendId);

            var friendRequest = user.SendFriendRequest(futureFriend);

            this.DbContext.SaveChanges();

            return friendRequest;
        }

        public Friend AcceptFriendRequest(string userId, string futureFriendId)
        {
            var user = new UserRepository(this.DbContext).GetUserById(userId);

            var friendRequest = this.GetFriendRequest(userId, futureFriendId);

            if (friendRequest == null)
            {
                throw HttpResponseExceptionHelper.Create("This friend doesn't exists", HttpStatusCode.BadRequest);
            }
            if (friendRequest.FutureFriendId != user.Id || friendRequest.UserId == user.Id)
            {
                throw HttpResponseExceptionHelper.Create("You cannot accept this friend request", HttpStatusCode.BadRequest);
            }
            if (friendRequest.Accepted)
            {
                throw HttpResponseExceptionHelper.Create("This friend request has already been accepted", HttpStatusCode.BadRequest);
            }

            var friend = user.AcceptFriendRequest(friendRequest);

            if (friend == null)
            {
                throw HttpResponseExceptionHelper.Create("Could not create new friend entity", HttpStatusCode.BadRequest);
            }

            this.DbContext.SaveChanges();

            return friend;
        }

        private FriendRequest GetFriendRequest(string user1Id, string user2Id)
        {
            var friendRequest = this.DbContext.FriendRequests.SingleOrDefault(
                x => (x.FutureFriendId == user1Id && x.UserId == user2Id)
                     || (x.FutureFriendId == user2Id && x.UserId == user1Id));

            return friendRequest;
        }

        public IEnumerable<FriendRequest> GetPendingFriendRequests(string userId)
        {
            return this.DbContext.FriendRequests
                .Where(x => !x.Accepted && x.UserId == userId).ToList();
        }

        public IEnumerable<ApplicationUser> GetFriends(string userId)
        {
            var friends = this.DbContext.Friends
                .Where(x => x.User1Id == userId || x.User2Id == userId).ToList()
                .Select(x => x.User1Id == userId ? x.User2 : x.User1);

            return friends;
        }

        public void DeleteFriend(string userId, string friendId)
        {
            var friend = this.DbContext.Friends
                .SingleOrDefault(x => (x.User1Id == userId && x.User2Id == friendId) || (x.User1Id == friendId && x.User2Id == userId));

            if (friend == null)
            {
                throw HttpResponseExceptionHelper.Create("Could not remove this user from your friends: user is not your friend", HttpStatusCode.BadRequest);
            }

            this.DbContext.Friends.Remove(friend);
            this.DbContext.SaveChanges();
        }
    }
}