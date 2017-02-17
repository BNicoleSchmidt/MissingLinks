using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using MissingLinks.Controllers;
using MissingLinks.Services;

namespace MissingLinks
{
    public class MvcApplication : HttpApplication
    {
        private static IContainer Container { get; set; }
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            PokeApiService.CacheAllData();

            var builder = new ContainerBuilder();

            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            builder.RegisterType<PokeApiService>().As<IPokeApiService>().SingleInstance();
            builder.RegisterType<LearnerHelper>().As<ILearnerHelper>();

            Container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(Container));
        }
    }
}
