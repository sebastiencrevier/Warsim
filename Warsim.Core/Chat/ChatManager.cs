using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Warsim.Core.Chat.Channels;
using Warsim.Core.Helpers.Http;
using Warsim.Core.Users;

namespace Warsim.Core.Chat
{
    public class ChatManager
    {
        public ConcurrentDictionary<Guid, Channel> Channels { get; }

        public ChatManager()
        {
            this.Channels = new ConcurrentDictionary<Guid, Channel>();
            this.CreatePublicChannel(null, "Général");
        }

        public Channel JoinChannel(WarsimUser user, Guid channelId)
        {
            var channel = this.GetChannelById(channelId);
            channel.AddUser(user);

            return channel;
        }

        public void LeaveChannel(WarsimUser user, Guid channelId)
        {
            this.GetChannelById(channelId).RemoveUser(user);
        }

        public void SendMessage(WarsimUser user, Channel channel, DateTime timestamp, string message)
        {
            // If the user is inside the channel
            if (channel.ActiveUsers.Any(x => x.UserId == user.UserId) && user.ActiveChannelId == channel.Id)
            {
                channel.Messages.Add(new Message
                {
                    UserId = user.UserId,
                    Content = message,
                    Timestamp = timestamp
                });

                return;
            }

            throw HttpResponseExceptionHelper.Create($"Could not send message: user {user.Username} isn't inside this channel", HttpStatusCode.BadRequest);
        }

        public Channel GetChannelById(Guid channelId)
        {
            if (!this.Channels.ContainsKey(channelId))
            {
                throw HttpResponseExceptionHelper.Create("This channel doesn't exists", HttpStatusCode.BadRequest);
            }

            return this.Channels[channelId];
        }

        public IList<Conversation> GetConversationsForUser(string userId)
        {
            var privateConversations = this.Channels.Values.Where(x => x.Type == ChannelType.PrivateConversation).Select(x => (Conversation)x);

            return privateConversations.Where(x => x.User1.UserId == userId || x.User2.UserId == userId).ToList();
        }

        public IList<PublicChannel> GetPublicChannels()
        {
            return this.Channels.Values.Where(x => x.Type == ChannelType.PublicChannel).Select(x => (PublicChannel)x).ToList();
        }

        public PublicChannel CreatePublicChannel(WarsimUser owner, string name)
        {
            var channel = new PublicChannel(name, owner);

            this.Channels.TryAdd(channel.Id, channel);

            return channel;
        }

        public Conversation CreateConversation(WarsimUser creator, WarsimUser secondUser)
        {
            var channel = new Conversation(creator, secondUser);

            this.Channels.TryAdd(channel.Id, channel);

            return channel;
        }
    }
}