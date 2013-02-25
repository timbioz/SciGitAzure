using System.Collections.Generic;
using Orchard;

namespace Onestop.Navigation.Breadcrumbs.Services
{
    /// <summary>
    /// Provides breadcrumbs path.
    /// </summary>
    public interface IBreadcrumbsProvider : IDependency
    {
        /// <summary>
        /// Order in which the provider will appear in the checking list.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Descriptors available for this provider.
        /// </summary>
        IEnumerable<BreadcrumbsProviderDescriptor> Descriptors { get; }

        /// <summary>
        /// Returns true if provider matches given name.
        /// </summary>
        bool Match(BreadcrumbsContext context);

        /// <summary>
        /// Builds breadcrumbs for a given item, 
        /// based on some underlying logic (eg. navigation, sitemap etc.)
        /// </summary>
        /// <returns>Build breadcrumbs instance.</returns>
        void Build(BreadcrumbsContext context);
    }
}