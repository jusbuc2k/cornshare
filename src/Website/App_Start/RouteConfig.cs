using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Website
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "LinkIndex",
                url: "Link/{sharetoken}/{action}/{fileid}",
                defaults: new { controller = "Link", action = "Index", fileid = UrlParameter.Optional }
            );

            //routes.MapRoute(
            //    name: "LinkDownload",
            //    url: "Link/{action}/{sharetoken}/{fileid}",
            //    defaults: new { controller = "Link", action = "Download", fileid = UrlParameter.Optional }
            //);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Create", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}