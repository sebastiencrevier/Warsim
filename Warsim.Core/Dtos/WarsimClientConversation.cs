using System;

using Warsim.Core.Chat.Channels;

namespace Warsim.Core.Dtos
{
    public class WarsimClientConversation
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public string Username { get; set; }

        public static WarsimClientConversation Map(Conversation conversation, string currentUserId)
        {
            var user = conversation.User1.UserId == currentUserId ? conversation.User2 : conversation.User1;

            return new WarsimClientConversation
            {
                Id = conversation.Id,
                UserId = user.UserId,
                Username = user.Username
            };
        }
    }
}