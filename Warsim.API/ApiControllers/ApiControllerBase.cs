using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

using Warsim.Core.DAL;
using Warsim.Core.Helpers.Http;
using Warsim.Core.Helpers.Jwt;
using Warsim.Core.Users;
using Warsim.Game;

namespace Warsim.API.ApiControllers
{
    public abstract class ApiControllerBase : ApiController
    {
        protected UserToken UserToken { get; }

        protected ApplicationDbContext DbContext { get; }

        protected GameManager GameManager { get; set; }

        protected WarsimUser WarsimUser => this.GameManager.GetUser(this.UserToken.UserId);

        protected ApiControllerBase(ApplicationDbContext dbContext)
        {
            this.UserToken = GetWarsimUserToken();
            this.DbContext = dbContext;
        }

        public static UserToken GetWarsimUserToken()
        {
            try
            {
                var token = HttpContext.Current.Request.Headers["Authorization"];

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw HttpResponseExceptionHelper.Create("Null or empty auth token", HttpStatusCode.Forbidden);
                }

                return JwtHelper.DecodeToken(token);
            }
            catch (Exception)
            {
                return null;
                //throw HttpResponseExceptionHelper.Create("Invalid auth token", HttpStatusCode.Forbidden);
            }
        }

        public static string GetQueryString(HttpRequestMessage request, string key)
        {
            var queryStrings = request.GetQueryNameValuePairs();

            if (queryStrings == null)
            {
                return null;
            }

            var match = queryStrings.FirstOrDefault(kv => string.Compare(kv.Key, key, StringComparison.OrdinalIgnoreCase) == 0);

            return string.IsNullOrEmpty(match.Value) ? null : match.Value;
        }
    }
}