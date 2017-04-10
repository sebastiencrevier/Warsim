using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using Warsim.Core.Game;
using Warsim.Core.Helpers.Jwt;
using Warsim.Events;
using Warsim.Events.Messages;
using Warsim.Events.Messages.Game;

namespace Warsim.Game.Udp
{
    public class UdpGameServer
    {
        private readonly EventProcessor _eventProcessor;

        public int BroadcastUpdateMsInterval { get; set; } = 50;

        public UdpClient Client { get; }

        public IPEndPoint EndPoint { get; }

        public ConcurrentDictionary<Guid, GameHost> GameHosts { get; }

        public bool IsRunning { get; set; }

        public UdpGameServer(IPEndPoint endpoint, ref ConcurrentDictionary<Guid, GameHost> gameHosts)
        {
            this.EndPoint = endpoint;
            this.Client = new UdpClient(endpoint);

            this.GameHosts = gameHosts;

            this._eventProcessor = new EventProcessor();
            this._eventProcessor.RegisterHandler(new Events.EventHandler<UpdatedGameStateMessage>(this.OnGameStateUpdate));
        }

        public void Start()
        {
            this.IsRunning = true;

            // Listen to messages
            Task.Run(async () =>
            {
                Console.WriteLine("Listening...");

                while (this.IsRunning)
                {
                    try
                    {
                        await this.ReceiveAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            });

            // Send periodic game state updates to players
            Task.Run(async () =>
            {
                while (this.IsRunning)
                {
                    try
                    {
                        this.BroadcastGameState(this.GameHosts.Values.ToArray());

                        await Task.Delay(TimeSpan.FromMilliseconds(this.BroadcastUpdateMsInterval));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            });
        }

        public void Stop()
        {
            this.IsRunning = false;
        }

        private void OnGameStateUpdate(UpdatedGameStateMessage msg, DateTime timestamp)
        {
            if (!this.GameHosts.ContainsKey(msg.GameId))
            {
                throw new Exception("This game id doesn't match any active game");
            }

            var game = this.GameHosts[msg.GameId];
            var userToken = JwtHelper.DecodeToken(msg.UserToken);

            if (userToken == null || game.Spectators.ContainsKey(userToken.UserId) || !game.Players.ContainsKey(userToken.UserId))
            {
                throw new Exception("This user is not allowed to change the map state");
            }

            // Update the current map state
            var stats = game.Map.UpdateScene(msg.UpdatedSceneObjects, userToken.UserId, timestamp);

            game.GameStatisticsUpdates.Add(stats);

            this.BroadcastGameState(game);
        }

        private void BroadcastGameState(params GameHost[] games)
        {
            foreach (var game in games)
            {
                var newMsg = new GameStateMessage
                {
                    GameId = game.Id,
                    SceneObjects = game.Map.SceneObjects
                };
         
                this.ReplyToAll(game.Id, newMsg);
            }
        }

        public async Task ReceiveAsync()
        {
            var received = await this.Client.ReceiveAsync();

            var eventBuilder = EventBuilder.Build(received.Buffer);

            this._eventProcessor.Process(eventBuilder.Event);
        }

        public void Reply(IEventMessage message, IPEndPoint endpoint)
        {
            var ev = EventBuilder.Build(message);
            var data = ev.ToByteArray();

            this.Client.Send(data, data.Length, endpoint);
        }

        public void ReplyToAll(Guid gameId, IEventMessage message)
        {
            var game = this.GameHosts[gameId];

            // Send to players and spectators
            foreach (var player in game.Everyone)
            {
                this.Reply(message, player.GameUdpEndPoint);
                Debug.WriteLine($"Broadcast to {player.Username} -> {player.GameUdpEndPoint}");
            }
        }
    }
}