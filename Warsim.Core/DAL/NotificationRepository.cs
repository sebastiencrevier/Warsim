using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Warsim.Core.Helpers.Http;
using Warsim.Core.Notifications;

namespace Warsim.Core.DAL
{
    public class NotificationRepository : ApplicationRepository
    {
        public NotificationRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }

        public void CreateNotification(Notification notification)
        {
            this.DbContext.Notifications.Add(notification);
            this.DbContext.SaveChanges();
        }

        public IEnumerable<Notification> GetNotifications(string userId)
        {
            return new UserRepository(this.DbContext).GetUserById(userId).Notifications;
        }

        public void DeleteNotification(string userId, Guid notificationId)
        {
            var notifications = this.GetNotifications(userId);

            var notification = notifications.SingleOrDefault(x => x.Id.Equals(notificationId));

            if (notification == null)
            {
                throw HttpResponseExceptionHelper.Create("This notification doesn't exists for this user", HttpStatusCode.BadRequest);
            }

            this.DbContext.Notifications.Remove(notification);
            this.DbContext.SaveChanges();
        }
    }
}