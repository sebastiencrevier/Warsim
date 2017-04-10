using System;
using System.Data.Entity;
using System.Web.Http;

using Warsim.Core.DAL;

namespace Warsim.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Set initializer for Entity Framework Database
            Database.SetInitializer(
                new DropCreateDatabaseIfModelChanges<ApplicationDbContext>()
            );

            config.Formatters.JsonFormatter
                .SerializerSettings
                .ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        }
    }
}