using System;
using System.Linq;
using System.Net;
using System.Web.Http;

using Warsim.API.ApiControllers.Messages;
using Warsim.API.Authentication;
using Warsim.API.Notifications;
using Warsim.Core.DAL;
using Warsim.Core.Dtos;
using Warsim.Game;

namespace Warsim.API.ApiControllers
{
    [JwtAuthorize]
    [RoutePrefix("api/chat")]
    public class ChatController : ApiControllerBase
    {
        public ChatController(GameManager gameManager, ApplicationDbContext dbContext)
            : base(dbContext)
        {
            this.GameManager = gameManager;
        }

        [HttpGet]
        [Route("channel")]
        public IHttpActionResult GetChannels()
        {
            var conversations = this.GameManager.ChatManager
                .GetConversationsForUser(this.UserToken.UserId)
                .Select(x => WarsimClientConversation.Map(x, this.UserToken.UserId));

            var channels = this.GameManager.ChatManager
                .GetPublicChannels()
                .Select(WarsimClientChannel.Map);

            var resp = new ChatChannelsResponseMessage
            {
                Conversations = conversations.ToList(),
                Channels = channels.ToList()
            };

            return this.Ok(resp);
        }

        [HttpPost]
        [Route("channel")]
        public IHttpActionResult CreatePublicChannel(ChatCreatePublicChannelMessage message)
        {
            var channel = this.GameManager.ChatManager.CreatePublicChannel(this.WarsimUser, message.ChannelName);

            NotificationManager.Create(this.GameManager, this.DbContext).SendPublicChannelInvite(channel, message.ParticipantsIds);

            return this.Ok(channel.Id);
        }

        [HttpPost]
        [Route("conversation")]
        public IHttpActionResult CreateConversation(ChatCreateConversationMessage message)
        {
            var existingConversation = this.GameManager.ChatManager
                .GetConversationsForUser(this.UserToken.UserId)
                .SingleOrDefault(x =>
                        (x.User1.UserId == this.UserToken.UserId && x.User2.UserId == message.SecondUserId) ||
                        (x.User1.UserId == message.SecondUserId && x.User2.UserId == this.UserToken.UserId)
                );

            // If the conversation already exists, respond to the user with a 302 Found
            if (existingConversation != null)
            {
                return this.Content(HttpStatusCode.Found, existingConversation);
            }

            var conversation = this.GameManager.ChatManager.CreateConversation(
                this.WarsimUser,
                this.GameManager.GetUser(message.SecondUserId));

            NotificationManager.Create(this.GameManager, this.DbContext).SendConversationInvite(conversation);

            // Respond with a 200 Ok
            return this.Ok(conversation.Id);
        }
    }
}