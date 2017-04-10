using System;

using Warsim.Core.Users;

namespace Warsim.API.Authentication.External
{
    public class TwitterAuthenticator : ExternalAuthenticator
    {
        public const string TwitterApiKey = "QcXwIWoxfZTSqLoUJMeXw7Qm1";
        public const string TwitterApiSecret = "DLSCjcYjqx7siJFi6dgFBZHxbsVHXpdafOvJBpqRz6Quaijgt2";
        public const string ProviderName = "Twitter";

        public static string GetUri()
        {
            var requestToken = GenerateRequestToken();

            var uri = $"https://api.twitter.com/oauth/authenticate?oauth_token={requestToken}";

            return uri;
        }

        public static string GenerateRequestToken()
        {
            var oauth = new Core.Helpers.OAuth.Manager();
            oauth["consumer_key"] = TwitterApiKey;
            oauth["consumer_secret"] = TwitterApiSecret;
            oauth["callback"] = $"{RedirectUri}?state=twitter";

            oauth.AcquireRequestToken("https://api.twitter.com/oauth/request_token", "POST");

            var requestToken = oauth["token"];

            return requestToken;
        }

        public static ApplicationUserLogin CreateUserLogin(string authToken, string authVerifier)
        {
            var oauth = new Core.Helpers.OAuth.Manager();
            oauth["consumer_key"] = TwitterApiKey;
            oauth["consumer_secret"] = TwitterApiSecret;
            oauth["token"] = authToken;

            oauth.AcquireAccessToken("https://api.twitter.com/oauth/access_token", "POST", authVerifier);

            var twitterDto = new TwitterDto
            {
                OAuthToken = oauth["token"],
                OAuthTokenSecret = oauth["token_secret"],
                Username = oauth["username"],
                UserId = oauth["userid"]
            };

            var userLogin = new ApplicationUserLogin
            {
                Provider = ProviderName,
                ProviderUserId = twitterDto.UserId,
                ProviderUsername = twitterDto.Username
            };

            return userLogin;
        }
    }
}