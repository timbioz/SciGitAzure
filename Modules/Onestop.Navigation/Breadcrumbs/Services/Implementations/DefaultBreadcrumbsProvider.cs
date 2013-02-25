using System;
using System.Collections.Generic;
using System.Linq;
using Orchard;
using Orchard.Environment.Extensions;

namespace Onestop.Navigation.Breadcrumbs.Services.Implementations
{
    /// <summary>
    /// Default provider returns current item only.
    /// </summary>
    [OrchardFeature("Onestop.Navigation.Breadcrumbs")]
    public class DefaultBreadcrumbsProvider : Component, IBreadcrumbsProvider
    {
        public const string ProviderName = "Default";

        public int Priority { get { return int.MinValue; } }

        public IEnumerable<BreadcrumbsProviderDescriptor> Descriptors
        {
            get
            {
                yield return new BreadcrumbsProviderDescriptor
                {
                    Name = ProviderName,
                    DisplayText = T("Default provider.").Text,
                };
            }
        }

        public bool Match(BreadcrumbsContext context)
        {
            return ProviderName.Equals(context.Provider, StringComparison.OrdinalIgnoreCase);
        }

        public void Build(BreadcrumbsContext context)
        {
            context.Breadcrumbs.Append(new Segment
            {
                Content = context.Content,
                Url = context.Content == null ? context.Paths.First() : null
            });
        }
    }
}