using System.Web.Mvc;
using System.Web.Routing;
using Sitecore.Pipelines;

namespace Enterspeed.Source.SitecoreCms.V8
{
    public class RegisterEnterspeedRoutes
    {
        public virtual void Process(PipelineArgs args)
        {
            RegisterRoute(RouteTable.Routes);
        }

        protected virtual void RegisterRoute(RouteCollection routes)
        {
            RouteTable.Routes.MapRoute("Enterspeed",
                "enterspeed/index", /* do not include a forward slash in front of the route */
                new { controller = "Enterspeed", action = "Index" } /* controller name should not have the "Controller" suffix */
            );
        }
    }
}