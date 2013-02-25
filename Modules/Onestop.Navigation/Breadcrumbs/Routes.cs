using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Routes;

namespace Onestop.Navigation.Breadcrumbs {
    [OrchardFeature("Onestop.Navigation.Breadcrumbs")]
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                    new RouteDescriptor {
                            Priority = 102, 
                            Route =
                                new Route(
                                "Admin/Navigation/Breadcrumbs", 
                                new RouteValueDictionary {
                                        { "area", "Onestop.Navigation" }, 
                                        { "controller", "BreadcrumbsAdmin" }, 
                                        { "action", "Index" },
                                }, 
                                new RouteValueDictionary(), 
                                new RouteValueDictionary { { "area", "Onestop.Navigation" } }, 
                                new MvcRouteHandler())
                    }
                };
        }
    }
}