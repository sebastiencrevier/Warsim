using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

using Warsim.Core.Game.Entities;
using Warsim.Core.Helpers.Http;
using Warsim.Core.Users;

namespace Warsim.Core.Game
{
    public class GameHost
    {
        private const int MaxPlayerCount = 4;

        public Guid Id { get; set; } = Guid.NewGuid();

        public Map Map { get; set; }

        public ConcurrentDictionary<string, WarsimUser> Players { get; set; } = new ConcurrentDictionary<string, WarsimUser>();

        public ConcurrentDictionary<string, WarsimUser> Spectators { get; set; } = new ConcurrentDictionary<string, WarsimUser>();

        public IList<WarsimUser> Everyone => this.Players.Values.Concat(this.Spectators.Values).ToList();

        public GameMode Mode { get; set; } = GameMode.Edition;

        public string OwnerId => this.Map.OwnerId;

        public bool IsPaused { get; set; }

        public CancellationTokenSource SyncMapTaskCancellationToken { get; set; } = new CancellationTokenSource();

        public IList<GameStatisticsUpdate> GameStatisticsUpdates { get; set; } = new List<GameStatisticsUpdate>();

        public GameHost(WarsimUser owner, Map map, string password = "")
        {
            this.Map = map;
            this.Map.OwnerId = owner.UserId;

            if (!this.Map.ValidatePassword(password))
            {
                throw HttpResponseExceptionHelper.Create("Wrong game password", HttpStatusCode.BadRequest);
            }
        
        }

        public GameHost(WarsimUser owner, string title, string description, string password = "")
        {
            this.Map = Map.CreateEmptyMap(owner.UserId, title, description, password);
        }

        public GameHost(WarsimUser owner, IList<Node> sceneObjects, string title, string description, string password = "")
        {
            this.Map = Map.CreateNewMap(sceneObjects, owner.UserId, title, description, password);
        }

        public void Join(WarsimUser user, string password = "")
        {
            if (this.Players.ContainsKey(user.UserId))
            {
                throw new GameException(2003, "The user is playing");
            }

            if (!this.Map.ValidatePassword(password))
            {
                throw new GameException(2004, "Wrong game password");
            }

            if (this.Players.Count >= MaxPlayerCount)
            {
                throw new GameException(2005, $"Game is full ({MaxPlayerCount} players)");
            }

            this.Players.TryAdd(user.UserId, user);
        }

        public void Spectate(WarsimUser user, string password = "")
        {
            if (this.Spectators.ContainsKey(user.UserId))
            {
                throw new GameException(2006, "The user is already spectating");
            }

            if (!this.Map.ValidatePassword(password))
            {
                throw new GameException(2004, "Wrong game password");
            }

            this.Spectators.TryAdd(user.UserId, user);
        }

        // Returns a bool stating if the game is still active or not
        public bool Leave(WarsimUser user)
        {
            this.Players.TryRemove(user.UserId, out user);

            // No more players, game is over now
            if (this.Players.Count == 0)
            {
                return false;
            }

            // If the owner leaves, find a new one
            if (this.OwnerId == user.UserId)
            {
                this.Map.OwnerId = this.Players.Keys.First();
            }

            return true;
        }
    }
}