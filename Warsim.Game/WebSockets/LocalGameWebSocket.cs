using System;

using Newtonsoft.Json;

using Warsim.Core.DAL;
using Warsim.Core.Game;
using Warsim.Core.Helpers.Jwt;
using Warsim.Core.Users;
using Warsim.Events;
using Warsim.Events.Messages.Game;

using WebSocketSharp;

namespace Warsim.Game.WebSockets
{
    public class LocalGameWebSocket : WebSocketBehaviorBase
    {
        private readonly EventProcessor _eventProcessor;

        private readonly GameManager _gameManager;
        private WarsimUser _warsimUser;
        private LocalGame _localGame;

        public LocalGameWebSocket(GameManager gameManager)
        {
            this._gameManager = gameManager;

            this._eventProcessor = new EventProcessor();
            this._eventProcessor.RegisterHandler(new Events.EventHandler<LocalGameStateMessage>(this.OnGameStateMessage));
        }

        private void OnGameStateMessage(LocalGameStateMessage msg, DateTime timestamp)
        {
            this._localGame.Map.SceneObjects = msg.SceneObjects;
        }

        protected override void OnWebSocketOpen()
        {
            var userToken = JwtHelper.DecodeToken(this.Context.QueryString.Get("auth_token"));
            var mapIdString = this.Context.QueryString.Get("map_id");

            Guid mapId;

            if (userToken == null || string.IsNullOrEmpty(mapIdString) || !Guid.TryParse(mapIdString, out mapId))
            {
                this.Sessions.CloseSession(this.ID, 2000, "Invalid query parameters");
                return;
            }

            this._warsimUser = this._gameManager.GetUser(userToken.UserId);

            if (this._warsimUser == null)
            {
                this.Sessions.CloseSession(this.ID, 2001, "This user isn't connected");
                return;
            }

            var mapRepo = new MapRepository(new ApplicationDbContext());

            var map = mapRepo.GetMapById(mapId);

            if (map == null)
            {
                this.Sessions.CloseSession(this.ID, 2002, "This map doesn't exist");
                return;
            }

            var password = this.Context.QueryString.Get("map_password");

            if (!map.ValidatePassword(password))
            {
                this.Sessions.CloseSession(this.ID, 2003, "Wrong map password");
                return;
            }

            if (map.IsLocked)
            {
                this.Sessions.CloseSession(this.ID, 2004, "This map is locked");
                return;
            }

            this._localGame = new LocalGame
            {
                Map = map,
                UserId = this._warsimUser.UserId
            };

            this._warsimUser.LocalGameWebSocketId = this.ID;
            this._gameManager.StartLocalGame(this._localGame);

            var localGameState = new LocalGameStateMessage
            {
                SceneObjects = map.SceneObjects
            };

            this.Sessions.SendTo(EventBuilder.Build(localGameState).Serialize(), this._warsimUser.LocalGameWebSocketId);
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
            this._warsimUser.LocalGameWebSocketId = string.Empty;

            this._gameManager.EndLocalGame(this._warsimUser.UserId);
        }
    }
}