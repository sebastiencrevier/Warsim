using System;

namespace Warsim.Core.Game
{
    public class GameStatisticsUpdate
    {
        public string UserId { get; set; }

        public int GameCreatedCount { get; set; }

        public int GameJoinedCount { get; set; }

        public int MapModifiedCount { get; set; }

        public int PostAddedCount { get; set; }

        public int WallAddedCount { get; set; }

        public int LineAddedCount { get; set; }
    }
}