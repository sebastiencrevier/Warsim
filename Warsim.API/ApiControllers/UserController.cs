using System;
using System.Linq;
using System.Web.Http;

using Warsim.API.ApiControllers.Messages;
using Warsim.API.Authentication;
using Warsim.Core.DAL;
using Warsim.Core.Dtos;
using Warsim.Core.Helpers.Blob;
using Warsim.Game;

namespace Warsim.API.ApiControllers
{
    [RoutePrefix("api/user")]
    public class UserController : ApiControllerBase
    {
        private readonly UserRepository _userRepo;

        public UserController(GameManager gameManager, ApplicationDbContext dbContext)
            : base(dbContext)
        {
            this._userRepo = new UserRepository(dbContext);
            this.GameManager = gameManager;
        }

        [HttpGet]
        [JwtAuthorize]
        public IHttpActionResult GetUsers()
        {
            var allUsers = this.DbContext.Users.ToList().Where(x => x.Id != this.UserToken.UserId).Select(WarsimClientUser.Map).ToList();

            foreach (var user in allUsers)
            {
                var warsimUser = this.GameManager.UserManager.GetUserById(user.Id);

                if (warsimUser != null)
                {
                    user.IsConnected = true;
                    user.GameId = warsimUser.ActiveGameId;
                }
            }

            var friendIds = new FriendshipRepository(this.DbContext).GetFriends(this.UserToken.UserId).Select(x => x.Id).ToList();

            var response = new UsersResponse
            {
                AllUsers = allUsers,
                FriendIds = friendIds
            };

            return this.Ok(response);
        }

        [HttpGet]
        [Route("list")]
        public IHttpActionResult GetUserListForWebsite()
        {
            var allUsers = this.DbContext.Users.ToList().Select(WarsimClientUser.Map).ToList();

            foreach (var user in allUsers)
            {
                var warsimUser = this.GameManager.UserManager.GetUserById(user.Id);

                if (warsimUser != null)
                {
                    user.IsConnected = true;
                    user.GameId = warsimUser.ActiveGameId;
                }
            }

            return this.Ok(allUsers.OrderBy(x => x.Username));
        }

        [HttpGet]
        [Route("{userId}")]
        public IHttpActionResult GetUserProfile(string userId)
        {
            // Get the current user info
            if (userId == "me" && this.UserToken != null)
            {
                userId = this.UserToken.UserId;
            }

            var user = this._userRepo.GetUserById(userId);

            var userProfile = new UserProfileMessage
            {
                Username = user.Username,
                Description = user.Description,
                GameCreatedCount = user.GameCreatedCount,
                GameJoinedCount = user.GameJoinedCount,
                MapModifiedCount = user.MapModifiedCount,
                PostAddedCount = user.PostAddedCount,
                WallAddedCount = user.WallAddedCount,
                LineAddedCount = user.LineAddedCount
            };

            var warsimUser = this.GameManager.UserManager.GetUserById(user.Id);

            if (warsimUser != null)
            {
                userProfile.IsConnected = true;
                userProfile.IsPlaying = warsimUser.IsPlaying;
            }

            return this.Ok(userProfile);
        }

        [HttpPut]
        [JwtAuthorize]
        public IHttpActionResult UpdateUserProfile(UpdateProfileMessage msg)
        {
            var user = this._userRepo.GetUserById(this.UserToken.UserId);

            // Update modified fields
            user.Description = msg.Description;

            // Upload the new avatar
            if (!string.IsNullOrEmpty(msg.AvatarInBase64))
            {
                AzureBlobStorageHelper.UploadAvatar(user.Id, Convert.FromBase64String(msg.AvatarInBase64));
            }

            this._userRepo.UpdateUser(user);

            return this.Ok();
        }

        [HttpPut]
        [JwtAuthorize]
        [Route("apple-token")]
        public IHttpActionResult UpdateAppleDeviceToken(string deviceToken)
        {
            var user = this._userRepo.GetUserById(this.UserToken.UserId);

            user.AppleDeviceToken = deviceToken;

            this._userRepo.UpdateUser(user);

            return this.Ok();
        }
    }
}