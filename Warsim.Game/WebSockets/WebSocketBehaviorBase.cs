using System;

using WebSocketSharp;
using WebSocketSharp.Server;

namespace Warsim.Game.WebSockets
{
    public abstract class WebSocketBehaviorBase : WebSocketBehavior
    {
        protected void InvokeAndCatchException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                this.Sessions.CloseSession(this.ID, CloseStatusCode.Normal, e.Message);
            }
        }

        protected abstract void OnWebSocketOpen();

        protected abstract void OnWebSocketMessage(MessageEventArgs e);

        protected abstract void OnWebSocketClose(CloseEventArgs e);

        protected override void OnOpen()
        {
            this.OnWebSocketOpen();
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            this.OnWebSocketMessage(e);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            this.OnWebSocketClose(e);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            this.Sessions.CloseSession(this.ID, CloseStatusCode.ServerError, e.Message);
        }
    }
}