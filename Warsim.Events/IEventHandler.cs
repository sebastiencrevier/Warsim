using System;

namespace Warsim.Events
{
    public interface IEventHandler
    {
        bool Handle(string message, DateTime timestamp);
    }
}