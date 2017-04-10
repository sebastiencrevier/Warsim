using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    public class UdpGameServerTest
    {
        private readonly EventProcessor _eventProcessor;

        public UdpClient Client { get; }

        public IPEndPoint EndPoint { get; }

       

        public bool IsRunning { get; set; }
        public Map map { get; set; }

        public IList<IPEndPoint> endPoints { get; set; }

        public UdpGameServerTest(IPEndPoint endpoint,Map game)
        {
            this.EndPoint = endpoint;
            this.Client = new UdpClient(endpoint);
            this._eventProcessor = new EventProcessor();
            this._eventProcessor.RegisterHandler(new Events.EventHandler<UpdatedGameStateMessage>(this.OnGameStateUpdate));
            this.map = game;
            endPoints = new List<IPEndPoint>();
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
                        this.BroadcastGameState();

                        await Task.Delay(TimeSpan.FromMilliseconds(1000));
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
            // Update the current map state
            //map.UpdateScene(msg.UpdatedSceneObjects, timestamp);

            this.BroadcastGameState();
        }

        private void BroadcastGameState()
        {
           
                var newMsg = new GameStateMessage
                {
                    SceneObjects = map.SceneObjects
                };

                this.ReplyToAll(newMsg);
            
        }

        public async Task ReceiveAsync()
        {
            var received = await this.Client.ReceiveAsync();

            var eventBuilder = EventBuilder.Build(received.Buffer);
            if(!endPoints.Contains(received.RemoteEndPoint))
              endPoints.Add(received.RemoteEndPoint);

            this._eventProcessor.Process(eventBuilder.Event);
        }

        public void Reply(IEventMessage message, IPEndPoint endpoint)
        {
            var ev = EventBuilder.Build(message);
            var data = ev.ToByteArray();

            this.Client.Send(data, data.Length, endpoint);
        }

        public void ReplyToAll( IEventMessage message)
        {

            // Send to players and spectators
            foreach (var endPoint in this.endPoints)
            {
                this.Reply(message, endPoint);
            }
        }
    }
}