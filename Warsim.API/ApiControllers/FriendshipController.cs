using System;
using System.Web.Http;

using Warsim.API.ApiControllers.Messages;
using Warsim.API.Authentication;
using Warsim.API.Notifications;
using Warsim.Core.DAL;
using Warsim.Game;

namespace Warsim.API.ApiControllers
{
    [JwtAuthorize]
    [RoutePrefix("api/friendship")]
    public class FriendshipController : ApiControllerBase
    {
        private readonly FriendshipRepository _friendshipRepo;

        public FriendshipController(GameManager gameManager, ApplicationDbContext dbContext)
            : base(dbContext)
        {
            this.GameManager = gameManager;
            this._friendshipRepo = new FriendshipRepository(dbContext);
        }

        [HttpPost]
        [Route("new")]
        public IHttpActionResult SendFriendRequest(NewFriendRequestMessage message)
        {
            var friendRequest = this._friendshipRepo.CreateFriendRequest(this.UserToken.UserId, message.FutureFriendId);

            NotificationManager.Create(this.GameManager, this.DbContext).SendFriendRequest(friendRequest);

            return this.Ok();
        }

        [HttpPost]
        [Route("accept")]
        public IHttpActionResult AcceptFriendRequest(AcceptFriendRequestMessage message)
        {
            this._friendshipRepo.AcceptFriendRequest(this.UserToken.UserId, message.FutureFriendId);

            return this.Ok();
        }

        [HttpDelete]
        public IHttpActionResult RemoveFriend(string friendId)
        {
            this._friendshipRepo.DeleteFriend(this.UserToken.UserId, friendId);

            return this.Ok();
        }
    }
}