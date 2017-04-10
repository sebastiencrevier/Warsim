using System;
using System.Collections.Generic;
using System.Net;

using Newtonsoft.Json;

using Warsim.API.ApiControllers.Messages;
using Warsim.Core.Helpers.Http;
using Warsim.Events;
using Warsim.Events.Messages.Chat;
using Warsim.Events.Messages.Game;

namespace Warsim.Client
{
    public class ClientTests
    {
        private const string Token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c2VySWQiOiI3OTU2MTMxMTY4NDMyNTQxMDcxMjIyMTExMzExNjEyMDgyMjM1OTExOTIwMjIzMDI1MjIyNjIyOSIsInVzZXJuYW1lIjoic2ViYXN0aWVuLmNyZXZpZXIiLCJleHBpcmF0aW9uIjoiXC9EYXRlKDE0ODA4MDMwMDkxMzYpXC8ifQ.1o34WLwmXkaQyFN8JxnCCSld9UPZRy0oiU3t6AzjF3A";
        private static UdpGameClient UdpGameClient;
        private static WebSocketClient UserWebSocket;

        private static void Main(string[] args)
        {
            // Dummy query to wake the server up
            var result = HttpRequestHelper.GetAsync("http://127.0.0.1:4321/api/game", Token).Result;

            WaitFor("Initialize client");

            UdpGameClient = new UdpGameClient("127.0.0.1", 5001);

            var udpPort = ((IPEndPoint)UdpGameClient.Client.Client.LocalEndPoint).Port;

            UserWebSocket = new WebSocketClient($"ws://127.0.0.1:6789/User/?auth_token={Token}&udp_port={udpPort}");
            UserWebSocket.Connect();

            //Create_game_with_new_map_and_join();
            Create_game_with_new_map_and_join();

            WaitFor("Disconnect client");
            UserWebSocket.Close();

            WaitFor("End");
        }

        private static void Create_game_with_new_map_and_join()
        {
            WaitFor("Create game");

            var postData = new CreateGameMessage
            {
                Title = "Wut a game",
                Password = "123"
            };

            var result = HttpRequestHelper.PostAsync("http://127.0.0.1:4321/api/game/create/empty", postData, Token).Result;
            var gameId = JsonConvert.DeserializeObject<Guid>(result.Content.ReadAsStringAsync().Result);

            WaitFor("Join game");
            var gameWebSocket = new WebSocketClient($"ws://127.0.0.1:6789/Game?auth_token={Token}&game_id={gameId}&game_password=123");
            gameWebSocket.Connect();

            WaitFor("Listen to game updates");

            UdpGameClient.RegisterOnGameStateChangeListener((message, time) =>
            {
                Console.WriteLine($"Scene update: {message.SceneObjects.Count} objects");
            });
            UdpGameClient.StartListening();

            WaitFor("Send game update");

            var gameUpdate = new UpdatedGameStateMessage
            {
                GameId = gameId,
                UserToken = Token,
                UpdatedSceneObjects = MapTests.GetNodes()
            };

            UdpGameClient.Send(gameUpdate);

            WaitFor("Exit game");

            UdpGameClient.StopListening();
            gameWebSocket.Close();
        }   

        private static void Create_channel_then_join_then_send_message()
        {
            WaitFor("Create channel");

            var postData = new ChatCreatePublicChannelMessage { ChannelName = "Channel de test", ParticipantsIds = new List<string>() };
            var result = HttpRequestHelper.PostAsync("http://127.0.0.1:4321/api/chat/channel", postData, Token).Result;

            var channelId = JsonConvert.DeserializeObject<Guid>(result.Content.ReadAsStringAsync().Result);

            WaitFor("Join channel");

            var chatWebSocket = new WebSocketClient($"ws://127.0.0.1:6789/Chat?auth_token={Token}&channel_id={channelId}");

            chatWebSocket.RegisterHandler((UserJoinedChannelMessage msg, DateTime timestamp) =>
            {
                Console.WriteLine($"Utilisateur connecté {msg.Username}");
            });
            chatWebSocket.OnCloseHandler = (o, args) =>
            {
                Console.WriteLine($"{args.Code}: {args.Reason}");
            };

            chatWebSocket.Connect();

            WaitFor("Send chat message");

            chatWebSocket.Send(EventBuilder.Build(new ChatMessageMessage
            {
                Content = "Hello channel!"
            }).Serialize());

            WaitFor("Exit channel");
            chatWebSocket.Close();
        }

        private static void WaitFor(string msg)
        {
            Console.WriteLine($"Appuyer sur une touche pour : {msg}");
            Console.ReadKey();
        }
    }
}