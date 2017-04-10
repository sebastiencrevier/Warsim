using System;
using System.Reflection;
using System.Web;
using System.Web.Http;

using Autofac;
using Autofac.Integration.WebApi;

using Warsim.Core.DAL;
using Warsim.Game;

namespace Warsim.API
{
    public class WebApiApplication : HttpApplication
    {
        private IContainer _container;

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            this.ConfigureAutofac(GlobalConfiguration.Configuration);
        }

        private void Application_End(object sender, EventArgs e)
        {
            this._container.Dispose();
        }

        private void ConfigureAutofac(HttpConfiguration config)
        {
            var builder = new ContainerBuilder();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterType<GameManager>().SingleInstance();
            builder.RegisterType<ApplicationDbContext>().AsSelf().InstancePerRequest();

            this._container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(this._container);
        }
    }
}