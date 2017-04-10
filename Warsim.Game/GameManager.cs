using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Warsim.Core.Chat;
using Warsim.Core.DAL;
using Warsim.Core.Game;
using Warsim.Core.Users;
using Warsim.Game.Udp;
using Warsim.Game.WebSockets;

namespace Warsim.Game
{
    public class GameManager : IDisposable
    {
        public int SyncMapSecInterval { get; set; } = 10;

        private readonly IList<LocalGame> _localGames = new List<LocalGame>();

        private readonly ConcurrentDictionary<Guid, GameHost> _gameHost = new ConcurrentDictionary<Guid, GameHost>();

        public ConcurrentDictionary<Guid, GameHost> GameHosts => this._gameHost;

        public WarsimUserManager UserManager { get; }

        public ChatManager ChatManager { get; }

        public WebSocketServices WebSocketServices { get; }

        public UdpGameServer UdpGameServer { get; set; }

        public const string ChatWsPath = "/Chat";

        public const string UserWsPath = "/User";

        public const string GameHostWsPath = "/Game";

        public const string LocalGameWsPath = "/LocalGame";

        public GameManager()
        {
            this.UserManager = new WarsimUserManager();
            this.ChatManager = new ChatManager();

            this.WebSocketServices = new WebSocketServices(6789);

            this.WebSocketServices.AddWebSocketService(UserWsPath, () => new UserWebSocket(this));
            this.WebSocketServices.AddWebSocketService(ChatWsPath, () => new ChatWebSocket(this));
            this.WebSocketServices.AddWebSocketService(GameHostWsPath, () => new GameHostWebSocket(this));
            this.WebSocketServices.AddWebSocketService(LocalGameWsPath, () => new LocalGameWebSocket(this));

            this.WebSocketServices.StartServer();

            this.UdpGameServer = new UdpGameServer(new IPEndPoint(IPAddress.Any, 5001), ref this._gameHost);
            this.UdpGameServer.Start();
        }

        public WarsimUser GetUser(string userId)
        {
            var user = this.UserManager.GetUserById(userId);

            if (user == null)
            {
                throw new Exception($"The user with id {userId} isn't connected");
            }

            return user;
        }

        public void StartGame(GameHost gameHost)
        {
            this.GameHosts.TryAdd(gameHost.Id, gameHost);

            // Launch task to sync the map AND its thumbnail
            Task.Run(async () => await this.SyncMapPeriodically(gameHost.Map.Id, false, new MapRepository(new ApplicationDbContext()), gameHost.SyncMapTaskCancellationToken.Token));
        }

        public void EndGame(GameHost gameHost)
        {
            // Stop syncing the current map
            gameHost.SyncMapTaskCancellationToken.Cancel();

            // Unlock the map for others to use it again
            gameHost.Map.IsLocked = false;

            var dbContext = new ApplicationDbContext();

            new MapRepository(dbContext).UpdateMap(gameHost.Map);

            var statsUpdate = gameHost.GameStatisticsUpdates.GroupBy(x => x.UserId)
                .Select(g => new GameStatisticsUpdate
                {
                    UserId = g.First().UserId,
                    GameCreatedCount = g.Sum(x => x.GameCreatedCount),
                    GameJoinedCount = g.Sum(x => x.GameJoinedCount),
                    MapModifiedCount = g.Sum(x => x.MapModifiedCount),
                    PostAddedCount = g.Sum(x => x.PostAddedCount),
                    WallAddedCount = g.Sum(x => x.WallAddedCount),
                    LineAddedCount = g.Sum(x => x.LineAddedCount)
                });

            foreach (var update in statsUpdate)
            {
                new UserRepository(dbContext).UpdateUserStatistics(update);
            }

            this.GameHosts.TryRemove(gameHost.Id, out gameHost);
        }

        public void StartLocalGame(LocalGame localGame)
        {
            this._localGames.Add(localGame);
            localGame.Map.IsLocked = true;
            localGame.Map.OwnerId = localGame.UserId;

            // Sync map
            new MapRepository(new ApplicationDbContext()).UpdateMap(localGame.Map);

            // Launch task to sync the map AND its thumbnail
            Task.Run(async () => await this.SyncMapPeriodically(localGame.Map.Id, true, new MapRepository(new ApplicationDbContext()), localGame.SyncMapTaskCancellationToken.Token));
        }

        public void EndLocalGame(string userId)
        {
            var localGame = this._localGames.SingleOrDefault(x => x.UserId == userId);

            if (localGame != null)
            {
                // Stop syncing the current map
                localGame.SyncMapTaskCancellationToken.Cancel();

                // Unlock the map for others to use it again
                localGame.Map.IsLocked = false;

                // Sync map for the last time
                new MapRepository(new ApplicationDbContext()).UpdateMap(localGame.Map);

                this._localGames.Remove(localGame);
            }
        }

        private async Task SyncMapPeriodically(Guid mapId, bool isLocalGame, MapRepository mapRepo, CancellationToken token = default(CancellationToken))
        {
            while (!token.IsCancellationRequested)
            {
                var map = isLocalGame ? this._localGames.Single(x => x.Map.Id.Equals(mapId)).Map : this.GameHosts.Values.Single(x => x.Map.Id.Equals(mapId)).Map;

                // Make sure it stays locked
                map.IsLocked = true;
                map.LastUpdated = DateTime.Now;

                mapRepo.UpdateMap(map);

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(this.SyncMapSecInterval), token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        public void Dispose()
        {
            this.UdpGameServer.Stop();
        }
    }
}