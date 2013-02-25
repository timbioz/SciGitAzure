using System.Collections.Generic;
using Onestop.Navigation.Breadcrumbs.Models;
using Onestop.Navigation.Breadcrumbs.Services;
using Onestop.Patterns.Services;

namespace Onestop.Navigation.Breadcrumbs.ViewModels {
    public class BreadcrumbsIndexViewModel {
        public BreadcrumbsIndexViewModel() {
            Matches = new Dictionary<string, PatternMatch>();
        }
        public IEnumerable<RoutePattern> Patterns { get; set; }
        public IDictionary<string, PatternMatch> Matches { get; set; }
        public IEnumerable<BreadcrumbsProviderDescriptor> Providers { get; set; }

        public string TestString { get; set; }
    }
}