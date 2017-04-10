using System;
using System.Collections.Generic;
using System.Net;

using Warsim.Core.Helpers.Http;
using Warsim.Core.Users;

namespace Warsim.Core.Chat.Channels
{
    public abstract class Channel
    {
        public ChannelType Type { get; set; }

        public Guid Id { get; } = Guid.NewGuid();

        public IList<WarsimUser> ActiveUsers { get; } = new List<WarsimUser>();

        public IList<Message> Messages { get; } = new List<Message>();

        private int MaxUserCount { get; }

        protected Channel(ChannelType type, int maxUserCount)
        {
            this.Type = type;
            this.MaxUserCount = maxUserCount;
        }

        public void AddUser(WarsimUser user)
        {
            if (this.ActiveUsers.Count + 1 > this.MaxUserCount)
            {
                throw HttpResponseExceptionHelper.Create(
                    $"Could not add user {user.Username}: " + 
                    $"channel already contains the maximum number of users ({this.MaxUserCount})", HttpStatusCode.BadRequest);
            }

            if (this.ActiveUsers.Contains(user))
            {
                throw HttpResponseExceptionHelper.Create($"User {user.Username} is already inside this channel", HttpStatusCode.BadRequest);
            }

            this.ActiveUsers.Add(user);
        }

        public bool RemoveUser(WarsimUser user)
        {
            if (this.ActiveUsers.Contains(user))
            {
                this.ActiveUsers.Remove(user);

                return true;
            }

            return false;
        }
    }
}