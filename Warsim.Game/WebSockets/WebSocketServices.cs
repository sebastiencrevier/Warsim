using System;

using WebSocketSharp.Server;

namespace Warsim.Game.WebSockets
{
    public class WebSocketServices : IDisposable
    {
        protected WebSocketServer WebSocketServer;

        public WebSocketServices(int port)
        {
            this.WebSocketServer = new WebSocketServer(port);
        }

        public void AddWebSocketService(string path, Func<WebSocketBehavior> initializer)
        {
            this.WebSocketServer.AddWebSocketService(path, initializer);
        }

        public void StartServer()
        {

            this.WebSocketServer.Start();
        }

        public WebSocketSessionManager GetSessions(string path)
        {
            return this.WebSocketServer.WebSocketServices[path].Sessions;
        }

        public void Broadcast(string path, string message)
        {
            this.GetSessions(path).Broadcast(message);
        }

        public void SendTo(string path, string message, params string[] sessionIds)
        {
            var sessions = this.GetSessions(path);

            foreach (var sessionId in sessionIds)
            {
                sessions.SendTo(message, sessionId);
            }
        }

        public void Dispose()
        {
            this.WebSocketServer.Stop();
        }
    }
}