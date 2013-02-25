using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Orchard.ContentManagement;

namespace Onestop.Navigation.Breadcrumbs.Services
{
    public class BreadcrumbsContext
    {
        public BreadcrumbsContext()
        {
            RouteValues = Enumerable.Empty<RouteValueDictionary>();
            Breadcrumbs = Breadcrumbs.Empty;
            Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets or sets a list of route value dictionaries to be checked by providers.
        /// Usually it contains a single entry, but in some cases, where some alternatives exist, there might be more.
        /// </summary>
        public IEnumerable<RouteValueDictionary> RouteValues { get; set; }

        /// <summary>
        /// Gets or sets a list of paths to be checked by providers.
        /// Usually it contains a single entry, but in some cases, where some alternatives exist, there might be more.
        /// </summary>
        public IEnumerable<string> Paths { get; set; }

        /// <summary>
        /// Contains additional properties that may be utilized by other breadcrumb-building components on the pipeline.
        /// </summary>
        public IDictionary<string, object> Properties { get; set; } 

        public IContent Content { get; set; }

        public string Provider { get; set; }
        public Breadcrumbs Breadcrumbs { get; set; }

        public bool Filtered { get; set; }
    }
}