using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using Warsim.Core.Chat.Channels;
using Warsim.Core.DAL;
using Warsim.Core.Game;
using Warsim.Core.Notifications;
using Warsim.Core.Users;
using Warsim.Core.Users.Friendships;
using Warsim.Events;
using Warsim.Events.Messages;
using Warsim.Events.Messages.Chat;
using Warsim.Events.Messages.Game;
using Warsim.Events.Messages.User;
using Warsim.Game;

namespace Warsim.API.Notifications
{
    public class NotificationManager
    {
        private readonly GameManager _gameManager;
        private readonly NotificationRepository _notificationRepo;

        public NotificationManager(GameManager gameManager, ApplicationDbContext dbContext)
        {
            this._gameManager = gameManager;
            this._notificationRepo = new NotificationRepository(dbContext);
        }

        public static NotificationManager Create(GameManager gameManager, ApplicationDbContext dbContext)
        {
            return new NotificationManager(gameManager, dbContext);
        }

        public void SendPublicChannelInvite(PublicChannel channel, IList<string> inviteeIds)
        {
            var msg = new ChannelInviteMessage
            {
                UserId = channel.Owner.UserId,
                Username = channel.Owner.Username,
                ChannelId = channel.Id,
                ChannelName = channel.Name
            };

            foreach (var invitee in inviteeIds)
            {
                var not = this.AddNotificationEntity(msg, invitee);

                msg.NotificationId = not.Id;
                this.SendWebSocketNotification(invitee, msg);
            }
        }

        public void SendConversationInvite(Conversation conversation)
        {
            var msg = new ConversationInviteMessage
            {
                UserId = conversation.User1.UserId,
                Username = conversation.User1.Username,
                ChannelId = conversation.Id
            };

            var not = this.AddNotificationEntity(msg, conversation.User2.UserId);

            msg.NotificationId = not.Id;
            this.SendWebSocketNotification(conversation.User2.UserId, msg);
        }

        public void SendGameInvite(GameHost game, WarsimUser inviter, string inviteeId)
        {
            var msg = new GameInviteMessage
            {
                GameId = game.Id,
                Title = game.Map.Title,
                UserId = inviter.UserId,
                Username = inviter.Username
            };

            var not = this.AddNotificationEntity(msg, inviteeId);

            msg.NotificationId = not.Id;
            this.SendWebSocketNotification(inviteeId, msg);
        }

        public void SendFriendRequest(FriendRequest friendRequest)
        {
            var msg = new FriendRequestMessage
            {
                UserId = friendRequest.UserId,
                Username = friendRequest.User.Username
            };

            var not = this.AddNotificationEntity(msg, friendRequest.FutureFriendId);

            msg.NotificationId = not.Id;
            this.SendWebSocketNotification(friendRequest.FutureFriendId, msg);
        }

        private void SendWebSocketNotification(string userId, IEventMessage message)
        {
            try
            {
                this._gameManager.WebSocketServices.SendTo(
                    GameManager.UserWsPath,
                    EventBuilder.Build(message).Serialize(),
                    this._gameManager.GetUser(userId).UserWebSocketId
                );
            }
            catch
            {
                // ignored
            }
        }

        private Notification AddNotificationEntity(IEventMessage msg, string userId)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Type = msg.GetType().Name,
                Content = JsonConvert.SerializeObject(msg),
                UserId = userId
            };

            this._notificationRepo.CreateNotification(notification);

            return notification;
        }
    }
}