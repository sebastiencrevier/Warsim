using System;

namespace Warsim.Core.Game
{
    public class GameException : Exception
    {
        public ushort Code { get; set; }

        public GameException(ushort code, string message)
            : base(message)
        {
            this.Code = code;
        }
    }
}