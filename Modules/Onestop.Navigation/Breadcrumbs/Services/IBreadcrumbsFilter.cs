using Orchard;

namespace Onestop.Navigation.Breadcrumbs.Services
{
    /// <summary>
    /// Describes a filter that is used to modify the generated breadcrumbs path.
    /// </summary>
    public interface IBreadcrumbsFilter : IDependency
    {
        /// <summary>
        /// Returns filter priority describing the order in which the filters will be applied.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Applies filter to given breadcrumbs instance.
        /// </summary>
        /// <returns></returns>
        void Apply(BreadcrumbsContext context);
    }
}