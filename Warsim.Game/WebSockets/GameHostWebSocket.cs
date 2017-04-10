using System;
using System.Linq;

using Newtonsoft.Json;

using Warsim.Core.DAL;
using Warsim.Core.Game;
using Warsim.Core.Helpers.Hash;
using Warsim.Core.Helpers.Jwt;
using Warsim.Core.Users;
using Warsim.Events;
using Warsim.Events.Messages.Game;

using WebSocketSharp;

namespace Warsim.Game.WebSockets
{
    public class GameHostWebSocket : WebSocketBehaviorBase
    {
        private readonly EventProcessor _eventProcessor;

        private readonly GameManager _gameManager;

        private WarsimUser _warsimUser;
        private GameHost _gameHost;

        public GameHostWebSocket(GameManager gameManager)
        {
            this._gameManager = gameManager;

            this._eventProcessor = new EventProcessor();
            this._eventProcessor.RegisterHandler(new Events.EventHandler<PauseGameMessage>(this.OnGamePause));
            this._eventProcessor.RegisterHandler(new Events.EventHandler<GameModeSwitchMessage>(this.OnGameModeSwitch));
            this._eventProcessor.RegisterHandler(new Events.EventHandler<EndGameMessage>(this.OnEndGame));
            this._eventProcessor.RegisterHandler(new Events.EventHandler<GamePrivacyChangeMessage>(this.OnGamePrivacyChange));
        }

        private void OnGamePrivacyChange(GamePrivacyChangeMessage msg, DateTime timestamp)
        {
            if (!this.IsGameOwner()) return;

            // If the game owner sets a password to a public game, it becomes private and we kick all other players
            if (!string.IsNullOrEmpty(msg.Password) && !this._gameHost.Map.IsPrivate)
            {
                foreach (var player in this._gameHost.Everyone.Where(x => x.UserId != this._gameHost.OwnerId))
                {
                    this.Sessions.CloseSession(player.GameHostWebSocketId, 2007, "This game is now private");
                }
            }

            this._gameHost.Map.Password = PasswordHash.CreateHash(msg.Password);
        }

        private void OnEndGame(EndGameMessage msg, DateTime timestamp)
        {
            if (!this.IsGameOwner()) return;

            this._gameManager.EndGame(this._gameHost);

            foreach (var player in this._gameHost.Everyone)
            {
                this.Sessions.CloseSession(player.GameHostWebSocketId, 2008, "The game owner has ended the game");
            }
        }

        private void OnGameModeSwitch(GameModeSwitchMessage msg, DateTime timestamp)
        {
            if (!this.IsGameOwner()) return;

            this._gameHost.Mode = msg.Mode;

            if (msg.Mode == GameMode.Edition)
            {
                this._gameHost.Map.RemoveRobots();
                this._gameHost.Map.DeselectAllObjects();
            }

                this.BroadcastToCurrentGame(EventBuilder.Build(msg).Serialize());
            }

        private void OnGamePause(PauseGameMessage msg, DateTime timestamp)
        {
            if (!this.IsGameOwner()) return;

            this._gameHost.IsPaused = msg.IsPaused;

            this.BroadcastToCurrentGame(EventBuilder.Build(msg).Serialize());
        }

        private bool IsGameOwner()
        {
            return this._warsimUser.UserId == this._gameHost.OwnerId;
        }

        protected override void OnWebSocketOpen()
        {
            var userToken = JwtHelper.DecodeToken(this.Context.QueryString.Get("auth_token"));
            var gameIdString = this.Context.QueryString.Get("game_id");

            Guid gameId;

            if (userToken == null || string.IsNullOrEmpty(gameIdString) || !Guid.TryParse(gameIdString, out gameId))
            {
                this.Sessions.CloseSession(this.ID, 2000, "Invalid query parameters");
                return;
            }

            if (!this._gameManager.GameHosts.ContainsKey(gameId))
            {
                this.Sessions.CloseSession(this.ID, 2001, "This game doesn't exist");
                return;
            }

            this._gameHost = this._gameManager.GameHosts[gameId];
            this._warsimUser = this._gameManager.GetUser(userToken.UserId);

            if (this._warsimUser == null)
            {
                this.Sessions.CloseSession(this.ID, 2002, "This user isn't connected");
                return;
            }

            var password = this.Context.QueryString.Get("game_password");
            var playerType = this.Context.QueryString.Get("player_type");

            if (playerType == "spectator")
            {
                // Start spectating, without telling other players
                try
                {
                    this._gameHost.Spectate(this._warsimUser, password);
                }
                catch (GameException e)
                {
                    this.Sessions.CloseSession(this.ID, e.Code, e.Message);
                    return;
                }
            }
            else
            {
                try
                {
                    this._gameHost.Join(this._warsimUser, password);
                }
                catch (GameException e)
                {
                    this.Sessions.CloseSession(this.ID, e.Code, e.Message);
                    return;
                }

                var msg = new PlayerConnectedMessage
                {
                    UserId = this._warsimUser.UserId,
                    Username = this._warsimUser.Username
                };

                this.BroadcastToCurrentGame(EventBuilder.Build(msg).Serialize());
            }

            var statsUpdate = new GameStatisticsUpdate { UserId = this._warsimUser.UserId };

            if (this.IsGameOwner())
            {
                statsUpdate.GameCreatedCount++;
            }
            else
            {
                statsUpdate.GameJoinedCount++;
            }

            new UserRepository(new ApplicationDbContext()).UpdateUserStatistics(statsUpdate);

            this._warsimUser.GameHostWebSocketId = this.ID;
            this._warsimUser.ActiveGameId = this._gameHost.Id;
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
            this._warsimUser.GameHostWebSocketId = string.Empty;
            this._warsimUser.ActiveGameId = Guid.Empty;

            // Owner has manually ended the game, the game is already erased from memory
            if (e.Code == 2008)
            {
                return;
            }

            // If the user is a spectator
            if (this._gameHost.Spectators.ContainsKey(this._warsimUser.UserId))
            {
                this._gameHost.Spectators.TryRemove(this._warsimUser.UserId, out this._warsimUser);
            }

            // If the user is a player
            var oldGameOwnerId = this._gameHost.OwnerId;
            var gameStillActive = this._gameHost.Leave(this._warsimUser);

            if (gameStillActive)
            {
                this._gameHost.Map.ResetStartArrow(this._warsimUser.UserId);
                this._gameHost.Map.DeselectUserObjects(this._warsimUser.UserId);

                if (this._gameHost.Mode == GameMode.Simulation)
                {
                    this._gameHost.Map.RemoveUserRobot(this._warsimUser.UserId);
                }

                var disconnectedMsg = new PlayerDisconnectedMessage
                {
                    UserId = this._warsimUser.UserId,
                    Username = this._warsimUser.Username
                };

                this.BroadcastToCurrentGame(EventBuilder.Build(disconnectedMsg).Serialize());

                // This means the owner of the game has changed
                if (oldGameOwnerId == this._warsimUser.UserId)
                {
                    var newOwnerMsg = new NewGameOwnerMessage();
                    var newOwner = this._gameManager.GetUser(this._gameHost.OwnerId);

                    // Send a message to tell the new game owner
                    this.Sessions.SendTo(EventBuilder.Build(newOwnerMsg).Serialize(), newOwner.GameHostWebSocketId);
                }
            }
            else
            {
                this._gameManager.EndGame(this._gameHost);
            }
        }

        private void BroadcastToCurrentGame(string message)
        {
            foreach (var player in this._gameHost.Everyone)
            {
                this.Sessions.SendTo(message, player.GameHostWebSocketId);
            }
        }
    }
}