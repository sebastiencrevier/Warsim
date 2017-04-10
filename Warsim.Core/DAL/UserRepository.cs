using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;

using Warsim.Core.Game;
using Warsim.Core.Helpers.Blob;
using Warsim.Core.Helpers.Http;
using Warsim.Core.Users;

namespace Warsim.Core.DAL
{
    public class UserRepository : ApplicationRepository
    {
        public UserRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }

        public ApplicationUser CreateUser(string username, string password)
        {
            var existingUser = this.DbContext.Users.SingleOrDefault(x => x.Username == username);

            if (existingUser != null)
            {
                throw HttpResponseExceptionHelper.Create("This username is already taken", HttpStatusCode.BadRequest);
            }

            var newUser = new ApplicationUser
            {
                Username = username,
                Id = ApplicationUserManager.GetUserHash(username),
                Password = ApplicationUserManager.HashUserPassword(password)
            };

            this.DbContext.Users.Add(newUser);
            this.DbContext.SaveChanges();

            AzureBlobStorageHelper.UploadDefaultAvatar(newUser.Id);

            return newUser;
        }

        public CreateExternalUserResult CreateExternalUser(ApplicationUserLogin userLogin)
        {
            if (string.IsNullOrEmpty(userLogin.ProviderUserId) || string.IsNullOrEmpty(userLogin.ProviderUsername))
            {
                throw HttpResponseExceptionHelper.Create($"{userLogin.Provider} account login failed", HttpStatusCode.BadRequest);
            }

            var result = new CreateExternalUserResult();

            // Check if the user login is already in use
            var existingUserLogin = this.DbContext.UserLogins
                .FirstOrDefault(x => x.Provider == userLogin.Provider && x.ProviderUserId == userLogin.ProviderUserId);

            // User already exists
            if (existingUserLogin != null)
            {
                result.User = this.GetUserById(existingUserLogin.UserId);
                result.UserCreated = false;

                return result;
            }

            // Take only the first part if email
            var username = userLogin.ProviderUsername.Split('@').First();

            // If username is already in use, tweak it until it's available
            while (this.DbContext.Users.FirstOrDefault(x => x.Username == username) != null)
            {
                username += new Random().Next(0, 9);
            }

            userLogin.UserId = ApplicationUserManager.GetUserHash(userLogin.ProviderUsername);

            var newUser = new ApplicationUser
            {
                Username = username,
                Id = userLogin.UserId,
                UserLogins = new List<ApplicationUserLogin> { userLogin }
            };

            this.DbContext.Users.Add(newUser);
            this.DbContext.SaveChanges();

            result.User = newUser;
            result.UserCreated = true;

            AzureBlobStorageHelper.UploadDefaultAvatar(newUser.Id);

            return result;
        }

        public ApplicationUser GetUserByUsername(string username)
        {
            var user = this.DbContext.Users.SingleOrDefault(x => x.Username == username);

            if (user == null)
            {
                throw HttpResponseExceptionHelper.Create("No user is associated to this username", HttpStatusCode.BadRequest);
            }

            return user;
        }

        public ApplicationUser GetUserById(string id)
        {
            var user = this.DbContext.Users.SingleOrDefault(x => x.Id == id);

            if (user == null)
            {
                throw HttpResponseExceptionHelper.Create("No user is associated to this id", HttpStatusCode.BadRequest);
            }

            return user;
        }

        public void DeleteUser(string userId)
        {
            var existingUser = this.GetUserById(userId);

            AzureBlobStorageHelper.DeleteAvatar(existingUser.Id);

            this.DbContext.Users.Remove(existingUser);
            this.DbContext.SaveChanges();
        }

        public void UpdateUser(ApplicationUser user)
        {
            this.DbContext.Users.Attach(user);
            this.DbContext.Entry(user).State = EntityState.Modified;

            this.DbContext.SaveChanges();
        }

        public void UpdateUserStatistics(GameStatisticsUpdate update)
        {
            var user = this.GetUserById(update.UserId);

            user.GameCreatedCount += update.GameCreatedCount;
            user.GameJoinedCount += update.GameJoinedCount;
            user.MapModifiedCount += update.MapModifiedCount;
            user.PostAddedCount += update.PostAddedCount;
            user.WallAddedCount += update.WallAddedCount;
            user.LineAddedCount += update.LineAddedCount;

            this.UpdateUser(user);
        }

        public class CreateExternalUserResult
        {
            public ApplicationUser User { get; set; }

            public bool UserCreated { get; set; }
        }
    }
}