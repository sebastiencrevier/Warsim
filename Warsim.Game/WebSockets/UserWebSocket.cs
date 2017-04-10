using System;
using System.Net;

using Warsim.Core.Helpers.Jwt;
using Warsim.Core.Users;
using Warsim.Events;
using Warsim.Events.Messages.User;

using WebSocketSharp;

namespace Warsim.Game.WebSockets
{
    public class UserWebSocket : WebSocketBehaviorBase
    {
        private readonly WarsimUserManager _userManager;
        private readonly GameManager _gameManager;
        private WarsimUser _warsimUser;

        public UserWebSocket(GameManager gameManager)
        {
            this._userManager = gameManager.UserManager;
            this._gameManager = gameManager;
        }

        protected override void OnWebSocketOpen()
        {
            var userToken = JwtHelper.DecodeToken(this.Context.QueryString.Get("auth_token"));
            var udpPort = this.Context.QueryString.Get("udp_port");

            // If invalid query parameters, close the connection
            if (userToken == null || string.IsNullOrEmpty(udpPort))
            {
                this.Sessions.CloseSession(this.ID, 2000, "Invalid query parameters");
                return;
            }

            // Add user to connected users
            if (this._userManager.Users.ContainsKey(userToken.UserId))
            {
                this.Sessions.CloseSession(this.ID, 2001, "User already connected");
                return;
            }

            this._warsimUser = this._userManager.ConnectUser(userToken.UserId, userToken.Username);
            this._warsimUser.UserWebSocketId = this.ID;

            // Save user UDP endpoint
            this._warsimUser.GameUdpEndPoint = new IPEndPoint(
                this.Context.UserEndPoint.Address,
                int.Parse(udpPort));

            // User has now connected to the game and is visible to other users
            var ev = EventBuilder.Build(new UserConnectedMessage
            {
                UserId = this._warsimUser.UserId,
                Username = this._warsimUser.Username
            });

            this.Sessions.Broadcast(ev.Serialize());
        }

        protected override void OnWebSocketMessage(MessageEventArgs e)
        {
        }

        protected override void OnWebSocketClose(CloseEventArgs e)
        {
            // Close all websocket connections if the user websocket connection is closed
            this._gameManager.WebSocketServices.GetSessions(GameManager.ChatWsPath).CloseSession(this._warsimUser.ChatWebSocketId);
            this._gameManager.WebSocketServices.GetSessions(GameManager.GameHostWsPath).CloseSession(this._warsimUser.GameHostWebSocketId);
            this._gameManager.WebSocketServices.GetSessions(GameManager.LocalGameWsPath).CloseSession(this._warsimUser.LocalGameWebSocketId);

            var ev = EventBuilder.Build(new UserDisconnectedMessage
            {
                UserId = this._warsimUser.UserId
            });

            this.Sessions.Broadcast(ev.Serialize());

            this._userManager.DisconnectUser(this._warsimUser.UserId);
        }
    }
}