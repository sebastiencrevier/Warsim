using System;
using System.Web;

using Newtonsoft.Json;

using Warsim.Core.Helpers.Http;
using Warsim.Core.Users;

namespace Warsim.API.Authentication.External
{
    public class FacebookAuthenticator : ExternalAuthenticator
    {
        public const string FacebookAppId = "342204052799709";
        public const string FacebookAppSecret = "9bdf77eb0bc7b426baf47addf38bf7d8";
        public const string ProviderName = "Facebook";

        public static string GetUri()
        {
            var uri = "https://www.facebook.com/dialog/oauth?scope=email" +
                      $"&state=facebook&client_id={FacebookAppId}" +
                      $"&redirect_uri={ExternalAuthenticator.RedirectUri}";
            return uri;
        }

        public static string RetrieveAccessToken(string code)
        {
            var url = $"https://graph.facebook.com/oauth/access_token?client_id={FacebookAppId}&redirect_uri={RedirectUri}&client_secret={FacebookAppSecret}&code={code}";

            var response = HttpRequestHelper.GetAsync(url).Result;
            var body = HttpRequestHelper.GetContent(response).Result;

            var token = HttpUtility.ParseQueryString(body).Get("access_token");

            return token;
        }

        public static ApplicationUserLogin CreateUserLogin(string accessToken)
        {
            var resp = HttpRequestHelper.GetAsync($"https://graph.facebook.com/v2.8/me?access_token={accessToken}&fields=id,email").Result;

            var json = resp.Content.ReadAsStringAsync().Result;
            var facebookDto = JsonConvert.DeserializeObject<FacebookDto>(json);

            var userLogin = new ApplicationUserLogin
            {
                Provider = ProviderName,
                ProviderUserId = facebookDto.Id,
                ProviderUsername = facebookDto.Email
            };

            return userLogin;
        }
    }
}