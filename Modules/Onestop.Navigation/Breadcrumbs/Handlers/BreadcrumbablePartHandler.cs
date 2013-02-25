using Onestop.Navigation.Breadcrumbs.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Onestop.Navigation.Breadcrumbs.Handlers
{
    [OrchardFeature("Onestop.Navigation.Breadcrumbs")]
    public class BreadcrumbablePartHandler : ContentHandler
    {
        public BreadcrumbablePartHandler(IRepository<BreadcrumbablePartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}