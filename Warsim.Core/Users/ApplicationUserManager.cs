using System;

using Warsim.Core.Helpers.Hash;
using Warsim.Core.Helpers.Jwt;

namespace Warsim.Core.Users
{
    public static class ApplicationUserManager
    {
        public static string GetUserHash(string username)
        {
            return Sha1Hash.GetSha1HashData(username);
        }

        public static string HashUserPassword(string password)
        {
            return PasswordHash.CreateHash(password);
        }

        public static bool ValidateUserPassword(string password, string passwordHash)
        {
            return PasswordHash.ValidatePassword(password, passwordHash);
        }

        public static string CreateToken(ApplicationUser user)
        {
            var token = new UserToken
            {
                UserId = user.Id,
                Username = user.Username
            };

            return JwtHelper.EncodeToken(token);
        }
    }
}