using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;

using Warsim.Core.Game;
using Warsim.Core.Helpers.Blob;
using Warsim.Core.Helpers.Graphics;
using Warsim.Core.Helpers.Http;

namespace Warsim.Core.DAL
{
    public class MapRepository : ApplicationRepository
    {
        public MapRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }

        public void CreateMap(Map map, bool isLocked = false)
        {
            AzureBlobStorageHelper.UploadMapThumbnail(map.Id.ToString(), MapThumbnailDrawer.Draw(map));

            map.IsLocked = isLocked;

            this.DbContext.Maps.Add(map);
            this.DbContext.SaveChanges();
        }

        public void UpdateMap(Map map)
        {
            AzureBlobStorageHelper.UploadMapThumbnail(map.Id.ToString(), MapThumbnailDrawer.Draw(map));

            this.DbContext.Maps.Attach(map);
            this.DbContext.Entry(map).State = EntityState.Modified;

            this.DbContext.SaveChanges();
        }

        public Map GetMapById(Guid mapId)
        {
            var map = this.DbContext.Maps.SingleOrDefault(x => x.Id.Equals(mapId));

            if (map == null)
            {
                throw HttpResponseExceptionHelper.Create("This map doesn't exists", HttpStatusCode.BadRequest);
            }

            return map;
        }

        public IEnumerable<Map> GetMaps()
        {
            return this.DbContext.Maps.ToList();
        }

        public void DeleteMap(Map map)
        {
            if (map.IsLocked)
            {
                throw HttpResponseExceptionHelper.Create("Cannot delete a locked map, unlock it first", HttpStatusCode.BadRequest);
            }

            AzureBlobStorageHelper.DeleteMapThumbnail(map.Id.ToString());

            this.DbContext.Maps.Remove(map);
            this.DbContext.SaveChanges();
        }
    }
}