using System;

namespace Warsim.Core.Notifications
{
    public class Notification
    {
        public Guid Id { get; set; }

        public string Type { get; set; }

        public string Content { get; set; }

        public string UserId { get; set; }
    }
}