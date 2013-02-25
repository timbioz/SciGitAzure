using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Onestop.Navigation.Models;
using Orchard.Mvc.Routes;

namespace Onestop.Navigation {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                    new RouteDescriptor {
                            Priority = 101, 
                            SessionState = SessionStateBehavior.Disabled,
                            Route =
                                new Route(
                                "Admin/Navigation/{menuId}/Children/{mode}", 
                                new RouteValueDictionary {
                                        { "area", "Onestop.Navigation" }, 
                                        { "controller", "MenuAdmin" }, 
                                        { "action", "GetChildren" },
                                        { "mode", DisplayMode.Current }
                                }, 
                                new RouteValueDictionary(), 
                                new RouteValueDictionary { { "area", "Onestop.Navigation" } }, 
                                new MvcRouteHandler())
                    },
                    new RouteDescriptor {
                            Priority = 101, 
                            SessionState = SessionStateBehavior.Disabled,
                            Route =
                                new Route(
                                "Admin/Navigation/{menuId}/Count/{mode}", 
                                new RouteValueDictionary {
                                        { "area", "Onestop.Navigation" }, 
                                        { "controller", "MenuAdmin" }, 
                                        { "action", "CountChildren" },
                                        { "mode", DisplayMode.Current }
                                }, 
                                new RouteValueDictionary(), 
                                new RouteValueDictionary { { "area", "Onestop.Navigation" } }, 
                                new MvcRouteHandler())
                    },
                    new RouteDescriptor {
                            Priority = 100, 
                            Route = new Route(
                                    "Admin/Navigation/{menuId}", 
                                    new RouteValueDictionary {
                                            { "area", "Onestop.Navigation" }, 
                                            { "controller", "MenuAdmin" }, 
                                            { "action", "Index" }, 
                                    }, 
                                    new RouteValueDictionary(), 
                                    new RouteValueDictionary { { "area", "Onestop.Navigation" } }, 
                                    new MvcRouteHandler())
                     }, 
                    new RouteDescriptor {
                            Priority = 100, 
                            Route = new Route(
                                "Admin/Navigation/{menuId}/CreateItem/{type}", 
                                new RouteValueDictionary {
                                        { "area", "Onestop.Navigation" }, 
                                        { "controller", "MenuAdmin" }, 
                                        { "action", "CreateItem" }, 
                                        { "type", "MenuItem" }
                                }, 
                                new RouteValueDictionary(), 
                                new RouteValueDictionary { { "area", "Onestop.Navigation" } }, 
                                new MvcRouteHandler())
                    }, 
                    new RouteDescriptor {
                            Priority = 101, 
                            Route = new Route(
                                "Admin/Navigation/{menuId}/Import", 
                                new RouteValueDictionary {
                                        { "area", "Onestop.Navigation" }, 
                                        { "controller", "ImportAdmin" }, 
                                        { "action", "Index" } 
                                }, 
                                new RouteValueDictionary(), 
                                new RouteValueDictionary { { "area", "Onestop.Navigation" } }, 
                                new MvcRouteHandler())
                    },
                    new RouteDescriptor {
                            Priority = 100, 
                            Route = new Route(
                                "Admin/Navigation/{menuId}/Preview/{versionNumber}", 
                                new RouteValueDictionary {
                                        { "area", "Onestop.Navigation" }, 
                                        { "controller", "MenuAdmin" }, 
                                        { "action", "Preview" }
                                }, 
                                new RouteValueDictionary(), 
                                new RouteValueDictionary { { "area", "Onestop.Navigation" } }, 
                                new MvcRouteHandler())
                    }, 
                    new RouteDescriptor {
                            Priority = 100, 
                            Route = new Route(
                                "Admin/Navigation/{menuId}/{action}/{itemId}", 
                                new RouteValueDictionary {
                                        { "area", "Onestop.Navigation" }, 
                                        { "controller", "MenuAdmin" }, 
                                        { "action", "Index" }
                                }, 
                                new RouteValueDictionary(), 
                                new RouteValueDictionary { { "area", "Onestop.Navigation" } }, 
                                new MvcRouteHandler())
                    }, 
                    new RouteDescriptor {
                            Priority = 100, 
                            Route =
                                new Route(
                                "Admin/Navigation/{menuId}/{action}", 
                                new RouteValueDictionary {
                                        { "area", "Onestop.Navigation" }, 
                                        { "controller", "MenuAdmin" }, 
                                        { "action", "Index" }
                                }, 
                                new RouteValueDictionary(), 
                                new RouteValueDictionary { { "area", "Onestop.Navigation" } }, 
                                new MvcRouteHandler())
                    }
                };
        }
    }
}