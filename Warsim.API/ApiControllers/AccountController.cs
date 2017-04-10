using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

using Warsim.API.ApiControllers.Messages;
using Warsim.API.Authentication;
using Warsim.API.Authentication.External;
using Warsim.Core.DAL;
using Warsim.Core.Helpers.Http;
using Warsim.Core.Users;

namespace Warsim.API.ApiControllers
{
    [RoutePrefix("api/account")]
    public class AccountController : ApiControllerBase
    {
        private readonly UserRepository _userRepo;

        public AccountController(ApplicationDbContext dbContext)
            : base(dbContext)
        {
            this._userRepo = new UserRepository(dbContext);
        }

        [HttpGet]
        [Route("external")]
        public IHttpActionResult GetExternalLoginUris()
        {
            return this.Ok(new
            {
                Facebook = FacebookAuthenticator.GetUri(),
                Google = GoogleAuthenticator.GetUri(),
                Twitter = TwitterAuthenticator.GetUri()
            });
        }

        [HttpGet]
        [Route("register/external")]
        public IHttpActionResult RegisterExternal(string state)
        {
            ApplicationUserLogin userLogin;

            state = state.ToLower();

            if (state == "facebook")
            {
                var token = FacebookAuthenticator.RetrieveAccessToken(GetQueryString(this.Request, "code"));
                userLogin = FacebookAuthenticator.CreateUserLogin(token);
            }
            else if (state == "google")
            {
                var token = GoogleAuthenticator.RetrieveAccessToken(GetQueryString(this.Request, "code"));
                userLogin = GoogleAuthenticator.CreateUserLogin(token);
            }
            else if (state == "twitter")
            {
                var authToken = GetQueryString(this.Request, "oauth_token");
                var authVerifier = GetQueryString(this.Request, "oauth_verifier");

                userLogin = TwitterAuthenticator.CreateUserLogin(authToken, authVerifier);
            }
            else
            {
                return this.BadRequest();
            }

            var result = this._userRepo.CreateExternalUser(userLogin);
            var userToken = ApplicationUserManager.CreateToken(result.User);

            // Returns the user token in text/plain format to help our deficient friend IE
            var resp = this.Request.CreateResponse();
            resp.Content = new StringContent(userToken, Encoding.UTF8, "text/plain");

            if (result.UserCreated)
            {
                resp.StatusCode = HttpStatusCode.Created;
                return this.ResponseMessage(resp);
            }

            resp.StatusCode = HttpStatusCode.OK;

            return this.ResponseMessage(resp);
        }

        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(RegisterMessage message)
        {
            if (!string.IsNullOrWhiteSpace(message.Username))
            {
                if (message.Username.Length > 20)
                {
                    throw HttpResponseExceptionHelper.Create("Username exceeds the 20 characters limit", HttpStatusCode.BadRequest);
                }
                if (message.Username.Contains(" "))
                {
                    throw HttpResponseExceptionHelper.Create("Username cannot contain whitespace", HttpStatusCode.BadRequest);
                }
            }
            if (string.IsNullOrWhiteSpace(message.Username) || string.IsNullOrWhiteSpace(message.Password))
            {
                throw HttpResponseExceptionHelper.Create("Register model is not valid", HttpStatusCode.BadRequest);
            }

            var user = this._userRepo.CreateUser(message.Username, message.Password);

            return this.Ok(ApplicationUserManager.CreateToken(user));
        }

        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(LoginMessage message)
        {
            var user = this._userRepo.GetUserByUsername(message.Username);

            if (user == null)
            {
                throw HttpResponseExceptionHelper.Create("Invalid username/password", HttpStatusCode.BadRequest);
            }

            if (!ApplicationUserManager.ValidateUserPassword(message.Password, user.Password))
            {
                throw HttpResponseExceptionHelper.Create("Invalid username/password", HttpStatusCode.BadRequest);
            }

            return this.Ok(ApplicationUserManager.CreateToken(user));
        }

        [HttpDelete]
        [JwtAuthorize]
        public IHttpActionResult DeleteUser()
        {
            this._userRepo.DeleteUser(this.UserToken.UserId);

            return this.Ok();
        }
    }
}