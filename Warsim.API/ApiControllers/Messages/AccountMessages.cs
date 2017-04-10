using System;

namespace Warsim.API.ApiControllers.Messages
{
    public class RegisterMessage
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }

    public class LoginMessage
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }

    public class RegisterExternalMessage
    {
        public string ProviderKey { get; set; }

        public string Provider { get; set; }
    }
}