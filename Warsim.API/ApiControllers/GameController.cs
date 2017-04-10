using System;
using System.Linq;
using System.Web.Http;

using Warsim.API.ApiControllers.Messages;
using Warsim.API.Authentication;
using Warsim.API.Notifications;
using Warsim.Core.DAL;
using Warsim.Core.Dtos;
using Warsim.Core.Game;
using Warsim.Core.Helpers.Hash;
using Warsim.Game;

namespace Warsim.API.ApiControllers
{
    [JwtAuthorize]
    [RoutePrefix("api/game")]
    public class GameController : ApiControllerBase
    {
        private readonly MapRepository _mapRepo;

        public GameController(GameManager gameManager, ApplicationDbContext dbContext)
            : base(dbContext)
        {
            this.GameManager = gameManager;
            this._mapRepo = new MapRepository(this.DbContext);
        }

        [HttpPost]
        [Route("create/empty")]
        public IHttpActionResult CreateGameWithEmptyMap(CreateGameMessage msg)
        {
            var gameHost = new GameHost(this.WarsimUser, msg.Title, msg.Description, msg.Password);

            this._mapRepo.CreateMap(gameHost.Map, true);
            this.GameManager.StartGame(gameHost);

            return this.Ok(gameHost.Id);
        }

        [HttpPost]
        [Route("create/new")]
        public IHttpActionResult CreateGameWithNewMap(CreateGameWithNewMapMessage msg)
        {
            var gameHost = new GameHost(this.WarsimUser, msg.SceneObjects, msg.Title, msg.Description, msg.Password);

            this._mapRepo.CreateMap(gameHost.Map, true);
            this.GameManager.StartGame(gameHost);

            return this.Ok(gameHost.Id);
        }

        [HttpPost]
        [Route("create/{mapId}")]
        public IHttpActionResult CreateGameWithExistingMap(CreateGameWithExistingMapMessage msg)
        {
            var map = this._mapRepo.GetMapById(msg.MapId);

            if (map.IsLocked)
            {
                return this.BadRequest("This map is already in use");
            }

            var gameHost = new GameHost(this.WarsimUser, map, msg.ExistingPassword);
            map.Password = PasswordHash.CreateHash(msg.Password);

            map.IsLocked = true;
            map.Title = msg.Title;
            map.Description = msg.Description;

            this._mapRepo.UpdateMap(map);
            this.GameManager.StartGame(gameHost);

            return this.Ok(gameHost.Id);
        }

        [HttpGet]
        public IHttpActionResult GetGames()
        {
            return this.Ok(this.GameManager.GameHosts.Values.Select(WarsimClientGame.Map));
        }

        [HttpGet]
        public IHttpActionResult GetGamesWithFilters(bool isPrivate, bool hasFriends, GameMode gameMode)
        {
            var friendIds = new FriendshipRepository(this.DbContext).GetFriends(this.WarsimUser.UserId).Select(x => x.Id);

            var games = this.GameManager.GameHosts.Values
                .Where(x => x.Map.IsPrivate == isPrivate);

            if (gameMode != GameMode.All)
            {
                games = games.Where(x => x.Mode == gameMode);
            }

            if (hasFriends)
            {
                games = games.Where(x => x.Players.Values.Any(y => friendIds.Contains(y.UserId)));
            }

            return this.Ok(games.Select(WarsimClientGame.Map));
        }

        [HttpPost]
        [Route("invite")]
        public IHttpActionResult SendGameInvitation(SendGameInviteMessage msg)
        {
            if (!this.WarsimUser.IsPlaying)
            {
                return this.BadRequest("You must be in a game to send a game invite");
            }

            var game = this.GameManager.GameHosts.Values.SingleOrDefault(x => x.Id.Equals(this.WarsimUser.ActiveGameId));

            NotificationManager.Create(this.GameManager, this.DbContext).SendGameInvite(game, this.WarsimUser, msg.InviteeId);

            return this.Ok();
        }

        [HttpDelete]
        [Route("{gameId}")]
        public IHttpActionResult DeleteGame(Guid gameId)
        {
            if (this.GameManager.GameHosts.ContainsKey(gameId))
            {
                this.GameManager.EndGame(this.GameManager.GameHosts[gameId]);
            }

            return this.Ok();
        }
    }
}