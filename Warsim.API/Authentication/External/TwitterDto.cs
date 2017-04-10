using System;

namespace Warsim.API.Authentication.External
{
    public class TwitterDto
    {
        public string OAuthToken { get; set; }

        public string OAuthTokenSecret { get; set; }

        public string Username { get; set; }

        public string UserId { get; set; }
    }
}