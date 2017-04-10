using System;
using System.Linq;

using Newtonsoft.Json;

using Warsim.Core.Chat;
using Warsim.Core.Chat.Channels;
using Warsim.Core.Dtos;
using Warsim.Core.Helpers.Jwt;
using Warsim.Core.Users;
using Warsim.Events;
using Warsim.Events.Messages.Chat;

using WebSocketSharp;

namespace Warsim.Game.WebSockets
{
    public class ChatWebSocket : WebSocketBehaviorBase
    {
        private readonly EventProcessor _eventProcessor;

        private readonly ChatManager _chatManager;
        private readonly GameManager _gameManager;

        private Channel _channel;
        private WarsimUser _warsimUser;

        public ChatWebSocket(GameManager gameManager)
        {
            this._chatManager = gameManager.ChatManager;
            this._gameManager = gameManager;

            this._eventProcessor = new EventProcessor();
            this._eventProcessor.RegisterHandler(new Events.EventHandler<ChatMessageMessage>(this.OnMessageSent));
            this._eventProcessor.RegisterHandler(new Events.EventHandler<CloseChannelMessage>(this.OnChannelCloseRequest));
        }

        private void OnChannelCloseRequest(CloseChannelMessage message, DateTime timestamp)
        {
            if (this._channel.Type == ChannelType.PublicChannel)
            {
                var publicChannel = (PublicChannel)this._channel;

                // If user is the channel owner, close it and kick everyone out
                if (publicChannel.Owner.UserId == this._warsimUser.UserId)
                {
                    this._chatManager.Channels.TryRemove(this._channel.Id, out this._channel);

                    foreach (var user in this._channel.ActiveUsers)
                    {
                        this.Sessions.CloseSession(user.ChatWebSocketId, 2002, "This channel has been closed by its owner");
                    }
                }
            }
        }

        private void OnMessageSent(ChatMessageMessage message, DateTime timestamp)
        {
            this._chatManager.SendMessage(this._warsimUser, this._channel, DateTime.Now, message.Content);

            message.UserId = this._warsimUser.UserId;
            message.Username = this._warsimUser.Username;

            this.BroadcastToChannel(EventBuilder.Build(message).Serialize());
        }

        protected override void OnWebSocketOpen()
        {
            var userToken = JwtHelper.DecodeToken(this.Context.QueryString.Get("auth_token"));
            var channelIdString = this.Context.QueryString.Get("channel_id");

            Guid channelId;

            // If invalid token, close the connection
            if (userToken == null || string.IsNullOrEmpty(channelIdString) || !Guid.TryParse(channelIdString, out channelId))
            {
                this.Sessions.CloseSession(this.ID, 2000, "Invalid query parameters");
                return;
            }

            try
            {
                this._warsimUser = this._gameManager.GetUser(userToken.UserId);
            }
            catch
            {
                this.Sessions.CloseSession(this.ID, 2001, "This user isn't connected");
                return;
            }

            this._channel = this._chatManager.JoinChannel(this._warsimUser, channelId);

            this._warsimUser.ChatWebSocketId = this.ID;
            this._warsimUser.ActiveChannelId = channelId;

            // Send current channel state to new user
            this.Send(EventBuilder.Build(new ChannelStateMessage
            {
                ActiveUsers = this._channel.ActiveUsers.Select(WarsimClientUser.Map).ToList(),
                Messages = this._channel.Messages
            }).Serialize());

            this.BroadcastToChannel(EventBuilder.Build(new UserJoinedChannelMessage
            {
                UserId = this._warsimUser.UserId,
                Username = this._warsimUser.Username
            }).Serialize());
        }

        protected override void OnWebSocketMessage(MessageEventArgs e)
        {
            if (e.IsText)
            {
                var ev = JsonConvert.DeserializeObject<Event>(e.Data);
                this._eventProcessor.Process(ev);
            }
        }

        protected override void OnWebSocketClose(CloseEventArgs e)
        {
            // If the channel has been closed
            if (this._chatManager.GetChannelById(this._channel.Id) == null)
            {
                return;
            }

            this._chatManager.LeaveChannel(this._warsimUser, this._channel.Id);

            var leftChannelEvent = EventBuilder.Build(new UserLeftChannelMessage
            {
                UserId = this._warsimUser.UserId,
                Username = this._warsimUser.Username
            });

            this.BroadcastToChannel(leftChannelEvent.Serialize());

            this._warsimUser.ChatWebSocketId = string.Empty;
            this._warsimUser.ActiveChannelId = Guid.Empty;
        }

        private void BroadcastToChannel(string message)
        {
            foreach (var user in this._channel.ActiveUsers)
            {
                this.Sessions.SendTo(message, user.ChatWebSocketId);
            }
        }
    }
}