using System;
using System.Web.Http;

using Warsim.API.ApiControllers.Messages;
using Warsim.API.Authentication;
using Warsim.Core.DAL;
using Warsim.Game;

namespace Warsim.API.ApiControllers
{
    [JwtAuthorize]
    [RoutePrefix("api/notification")]
    public class NotificationController : ApiControllerBase
    {
        private readonly NotificationRepository _notificationRepo;

        public NotificationController(GameManager gameManager, ApplicationDbContext dbContext)
            : base(dbContext)
        {
            this.GameManager = gameManager;
            this._notificationRepo = new NotificationRepository(this.DbContext);
        }

        // Only one time on connection
        [HttpGet]
        public IHttpActionResult GetNotifications()
        {
            return this.Ok(this._notificationRepo.GetNotifications(this.UserToken.UserId));
        }

        [HttpPost]
        public IHttpActionResult AcknowledgeNotification(AcknowledgeNotificationMessage msg)
        {
            this._notificationRepo.DeleteNotification(this.UserToken.UserId, msg.NotificationId);

            return this.Ok();
        }
    }
}