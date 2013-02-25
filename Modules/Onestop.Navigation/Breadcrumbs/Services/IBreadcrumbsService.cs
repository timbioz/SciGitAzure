using System.Collections.Generic;
using System.Web.Routing;
using Onestop.Navigation.Breadcrumbs.Models;
using Orchard;
using Orchard.ContentManagement;

namespace Onestop.Navigation.Breadcrumbs.Services
{
    public interface IBreadcrumbsService : IDependency
    {
        IEnumerable<IBreadcrumbsProvider> GetProviders();
        IEnumerable<BreadcrumbsProviderDescriptor> GetProviderDescriptors();
        Breadcrumbs Build(IContent item = null, RouteValueDictionary routeData = null, string path = null);
        BreadcrumbsProviderDescriptor GetDefaultProvider();

        IEnumerable<RoutePattern> GetPatterns();
        void AddPattern(string pattern, string provider);
        void DeletePattern(int id);

        string GetUrl(IContent content);
    }
}