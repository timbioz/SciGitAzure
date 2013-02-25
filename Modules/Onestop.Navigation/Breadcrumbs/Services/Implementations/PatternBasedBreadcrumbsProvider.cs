using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Onestop.Patterns.Services;
using Orchard;
using Orchard.Environment.Extensions;

namespace Onestop.Navigation.Breadcrumbs.Services.Implementations
{
    /// <summary>
    /// Default provider uses Container - Containable relation to build breadcrumbs.
    /// </summary>
    [OrchardFeature("Onestop.Navigation.Breadcrumbs")]
    public class PatternBasedBreadcrumbsProvider : Component, IBreadcrumbsProvider
    {
        private readonly IPatternService _patterns;
        private readonly IWorkContextAccessor _accessor;
        public const string ProviderName = "Pattern";

        public int Priority { get { return int.MaxValue; } }

        public IEnumerable<BreadcrumbsProviderDescriptor> Descriptors {
            get { return Enumerable.Empty<BreadcrumbsProviderDescriptor>(); }
        }

        private IBreadcrumbsService BreadcrumbsService
        {
            get { return _accessor.GetContext().Resolve<IBreadcrumbsService>(); }
        }

        public PatternBasedBreadcrumbsProvider(
            IPatternService patterns, 
            IWorkContextAccessor accessor)
        {
            _patterns = patterns;
            _accessor = accessor;
        }

        public bool Match(BreadcrumbsContext context)
        {
            var patterns = BreadcrumbsService.GetPatterns();

            // Look up a provider matching the current URL
            PatternMatch matchResult = null;
            var match = patterns
                .ToList()
                .FirstOrDefault(p => context.Paths.Any(path => _patterns.TryMatch(path, p.Pattern, out matchResult)));

            // Set the provider to the matching one and return false, which means
            // continuing with provider matching.
            if (match != null)
            {
                IDictionary<string, object> groups = new ExpandoObject();
                context.Provider = match.Provider;

                foreach (var group in matchResult.Groups)
                    groups.Add(group.Key, group.Value);

                context.Properties["Groups"] = groups;
                context.Properties["Pattern"] = match.Pattern;
            }

            return false;
        }

        public void Build(BreadcrumbsContext context){ }
    }
}