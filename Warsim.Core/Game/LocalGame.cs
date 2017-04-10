using System;
using System.Threading;

namespace Warsim.Core.Game
{
    public class LocalGame
    {
        public string UserId { get; set; }

        public Map Map { get; set; }

        public CancellationTokenSource SyncMapTaskCancellationToken { get; set; } = new CancellationTokenSource();
    }
}