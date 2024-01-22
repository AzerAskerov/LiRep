using Autofac;
using Autofac.Integration.Mvc;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode;

namespace Libra.Web
{
    public class LibraApplication : HttpApplication
    {
        protected void Application_Start()
        {
            RegisterRoutes(RouteTable.Routes);
            GlobalFilters.Filters.Add(new AuthorizeAttribute());
            InitAutofac();

            ModelBinders.Binders.Add(typeof(decimal), new DecimalModelBinder());
            ModelBinders.Binders.Add(typeof(decimal?), new DecimalModelBinder());
            ModelBinders.Binders.Add(typeof(DateTime), new DateTimeModelBinder());
            ModelBinders.Binders.Add(typeof(DateTime?), new DateTimeModelBinder());
        }

        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }

        private static void InitAutofac()
        {
            var builder = new ContainerBuilder();

            /* Register Autofac modules from Libra libs */
            var executingFile = Assembly.GetExecutingAssembly().CodeBase;
            if (!string.IsNullOrEmpty(executingFile))
            {
                var executingPath = Path.GetDirectoryName(executingFile)
                    .Replace("file:\\", "");
                if (!string.IsNullOrEmpty(executingPath))
                {

                    builder.RegisterAssemblyModules(new DirectoryInfo(executingPath)
                        .GetFiles("Libra.*.dll")
                        .Select(f => Assembly.LoadFrom(f.FullName))
                        .ToArray());
                }
            }

            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = Config.AuthenticationType,
                AuthenticationMode = AuthenticationMode.Active,
                LoginPath = new PathString("/Account/Login"),
                ExpireTimeSpan = TimeSpan.FromMinutes(Config.SessionTimeout),
                SlidingExpiration = true
            });
        }
    }
}
