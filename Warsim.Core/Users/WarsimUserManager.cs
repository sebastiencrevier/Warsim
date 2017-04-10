using System;
using System.Collections.Concurrent;

namespace Warsim.Core.Users
{
    public class WarsimUserManager
    {
        public ConcurrentDictionary<string, WarsimUser> Users = new ConcurrentDictionary<string, WarsimUser>();

        public WarsimUser ConnectUser(string userId, string username)
        {
            var user = new WarsimUser(userId, username);

            this.Users.TryAdd(user.UserId, user);

            return user;
        }

        public void DisconnectUser(string userId)
        {
            if (this.Users.ContainsKey(userId))
            {
                WarsimUser warsimUser;

                this.Users.TryRemove(userId, out warsimUser);
            }
        }

        public WarsimUser GetUserById(string userId)
        {
            if (!this.Users.ContainsKey(userId))
            {
                return null;
            }

            return this.Users[userId];
        }
    }
}