using System;
using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Environment.Extensions;

namespace Onestop.Navigation.Breadcrumbs.Services.Implementations
{
    /// <summary>
    /// Default provider uses Container - Containable relation to build breadcrumbs.
    /// </summary>
    [OrchardFeature("Onestop.Navigation.Breadcrumbs")]
    public class ContainerBreadcrumbsProvider : Component, IBreadcrumbsProvider
    {
        public const string ProviderName = "Container";

        public int Priority { get { return 0; } }

        public IEnumerable<BreadcrumbsProviderDescriptor> Descriptors {
            get {
                yield return new BreadcrumbsProviderDescriptor {
                    Name = ProviderName, 
                    DisplayText = T("Container-based provider.").Text,
                };
            }
        }

        public bool Match(BreadcrumbsContext context)
        {
            return ProviderName.Equals(context.Provider, StringComparison.OrdinalIgnoreCase);
        }

        public void Build(BreadcrumbsContext context)
        {
            if (context.Content == null) return;

            var item = context.Content.As<ICommonPart>();

            while (item != null) {
                context.Breadcrumbs.Prepend(new Segment { Content = item });
                item = item.Container.As<ICommonPart>();
            }
        }
    }
}