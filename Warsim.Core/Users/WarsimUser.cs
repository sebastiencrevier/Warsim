using System;
using System.Net;

namespace Warsim.Core.Users
{
    public class WarsimUser
    {
        public WarsimUser(string userId, string username)
        {
            this.UserId = userId;
            this.Username = username;
        }

        public string UserId { get; set; }

        public string Username { get; set; }

        public string UserWebSocketId { get; set; }

        public string ChatWebSocketId { get; set; }

        public string GameHostWebSocketId { get; set; }

        public string LocalGameWebSocketId { get; set; }

        public IPEndPoint GameUdpEndPoint { get; set; }

        public bool IsPlaying => !string.IsNullOrEmpty(this.GameHostWebSocketId);

        public Guid ActiveGameId { get; set; }

        public Guid ActiveChannelId { get; set; }
    }
}