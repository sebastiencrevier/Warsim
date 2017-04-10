using System;

namespace Warsim.Core.Users
{
    public class ApplicationUserLogin
    {
        public string UserId { get; set; }

        public string Provider { get; set; }

        public string ProviderUserId { get; set; }

        public string ProviderUsername { get; set; }
    }
}