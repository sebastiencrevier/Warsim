using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Newtonsoft.Json;

using Warsim.Core.Helpers.Http;
using Warsim.Core.Users;

namespace Warsim.API.Authentication.External
{
    public class GoogleAuthenticator : ExternalAuthenticator
    {
        public const string GoogleAppId = "1067324299084-qbn5l43dhp6vjo7lajfp1uoqn02vu754.apps.googleusercontent.com";
        public const string GoogleAppSecret = "coualKy04pNlsgtCWNorSaUf";
        public const string ProviderName = "Google";

        public static string GetUri()
        {
            var uri = "https://accounts.google.com/o/oauth2/v2/auth?scope=email%20profile&state=google&response_type=code" +
                      $"&redirect_uri={RedirectUri}&client_id={GoogleAppId}";

            return uri;
        }

        public static string RetrieveAccessToken(string code)
        {
            var content = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", GoogleAppId },
                { "client_secret", GoogleAppSecret },
                { "redirect_uri", RedirectUri },
                { "grant_type", "authorization_code" }
            };

            var encodedContent = new FormUrlEncodedContent(content);

            var response = new HttpClient().PostAsync("https://www.googleapis.com/oauth2/v4/token", encodedContent).Result;
            var json = response.Content.ReadAsStringAsync().Result;

            var body = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            var token = body["access_token"];

            return token;
        }

        public static ApplicationUserLogin CreateUserLogin(string accessToken)
        {
            var resp = HttpRequestHelper.GetAsync($"https://www.googleapis.com/plus/v1/people/me?fields=id%2Cemails%2Fvalue&access_token={accessToken}").Result;

            var json = resp.Content.ReadAsStringAsync().Result;
            var googleDto = JsonConvert.DeserializeObject<GoogleDto>(json);

            var userLogin = new ApplicationUserLogin
            {
                Provider = ProviderName,
                ProviderUserId = googleDto.Id,
                ProviderUsername = googleDto.Emails.FirstOrDefault()?.Value
            };

            return userLogin;
        }
    }
}