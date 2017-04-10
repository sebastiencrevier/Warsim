using System;
using System.Linq;
using System.Net;
using System.Web.Http;

using Warsim.API.ApiControllers.Messages;
using Warsim.API.Authentication;
using Warsim.Core.DAL;
using Warsim.Core.Game;
using Warsim.Core.Helpers.Hash;
using Warsim.Game;

namespace Warsim.API.ApiControllers
{
    [RoutePrefix("api/map")]
    public class MapController : ApiControllerBase
    {
        private readonly MapRepository _mapRepo;

        public MapController(GameManager gameManager, ApplicationDbContext dbContext)
            : base(dbContext)
        {
            this.GameManager = gameManager;
            this._mapRepo = new MapRepository(this.DbContext);
        }

        // After offline edit of a map, sync it back with the server and overwrite any modifications
        [HttpPut]
        [JwtAuthorize]
        [Route("{mapId}/sync")]
        public IHttpActionResult SyncMap(SyncMapMessage msg)
        {
            Map map;

            try
            {
                map = this._mapRepo.GetMapById(msg.MapId);
            }
            catch
            {
                // If map doesn't exists, create it and return a 201 Created
                map = Map.CreateNewMap(msg.SceneObjects, this.UserToken.UserId, $"Carte de {this.UserToken.Username}", "");
                map.Id = msg.MapId;

                this._mapRepo.CreateMap(map);

                return this.Created("", map);
            }

            // If map exists ...
            if (!map.ValidatePassword(msg.Password))
            {
                return this.BadRequest("Wrong map password");
            }

            // If the map is locked and the user is not the map owner, return a 409 Conflict
            if (map.IsLocked && map.OwnerId != this.UserToken.UserId)
            {
                return this.Content(HttpStatusCode.Conflict, "Can't sync map: this map is already in use");
            }

            // Force map overwrite!!
            map.SceneObjects = msg.SceneObjects;
            map.OwnerId = this.UserToken.UserId;
            map.IsLocked = false;
            map.LastUpdated = DateTime.Now;

            this._mapRepo.UpdateMap(map);

            return this.Ok();
        }

        [HttpPost]
        [JwtAuthorize]
        [Route("create")]
        public IHttpActionResult CreateEmptyMap()
        {
            var map = Map.CreateEmptyMap(this.UserToken.UserId, $"Carte de {this.UserToken.Username}", "");

            this._mapRepo.CreateMap(map);

            return this.Ok(map);
        }

        [HttpGet]
        [Route("{mapId}")]
        public IHttpActionResult GetMap(Guid mapId)
        {
            return this.Ok(this._mapRepo.GetMapById(mapId));
        }

        [HttpGet]
        [JwtAuthorize]
        [Route("available")]
        public IHttpActionResult GetAvailableMaps()
        {
            // Returns unlocked maps for the current user
            return this.Ok(this._mapRepo.GetMaps().Where(x => !x.IsLocked || x.OwnerId == this.UserToken.UserId));
        }

        [HttpGet]
        public IHttpActionResult GetMaps()
        {
            return this.Ok(this._mapRepo.GetMaps().OrderByDescending(x => x.LastUpdated));
        }

        [HttpPost]
        [JwtAuthorize]
        [Route("update")]
        public IHttpActionResult UpdateMap(UpdateExistingMapMessage msg)
        {
            var map = this._mapRepo.GetMapById(msg.MapId);

            if (map.IsLocked)
            {
                return this.BadRequest("This map is already in use");
            }

            if (this.UserToken.UserId != map.OwnerId)
            {
                return this.BadRequest("You can't edit a map that you don't own");
            }

            map.Title = msg.Title;
            map.Description = msg.Description;
            map.Password = PasswordHash.CreateHash(msg.Password);

            this._mapRepo.UpdateMap(map);

            return this.Ok();
        }

        [HttpDelete]
        [JwtAuthorize]
        [Route("{mapId}")]
        public IHttpActionResult DeleteMap(Guid mapId)
        {
            var map = this._mapRepo.GetMapById(mapId);

            if (this.UserToken.UserId != map.OwnerId)
            {
                return this.BadRequest("You can't delete a map that you don't own");
            }

            this._mapRepo.DeleteMap(map);

            return this.Ok();
        }
    }
}