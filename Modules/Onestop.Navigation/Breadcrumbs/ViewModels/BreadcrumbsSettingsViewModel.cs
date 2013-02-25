using System.Collections.Generic;
using Onestop.Navigation.Breadcrumbs.Services;

namespace Onestop.Navigation.Breadcrumbs.ViewModels
{
    public class BreadcrumbsSettingsViewModel
    {
        public string DefaultProvider { get; set; }
        public bool UseDefault { get; set; }
        public IEnumerable<BreadcrumbsProviderDescriptor> Providers { get; set; }
        public dynamic Preview { get; set; }
        public BreadcrumbsProviderDescriptor SiteDefaultProvider { get; set; }
    }
}