﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AdminPanel
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Dashboard", action = "Index", id = UrlParameter.Optional }
            );


            routes.MapRoute(
                "DeleteMedia",                                              // Route name
                "DelImage/{id}/{imageid}/1",                           // URL with parameters
                new { controller = "", action = "DelImage", id = "", imageid = "" }  // Parameter defaults
            );



        }
    }
}