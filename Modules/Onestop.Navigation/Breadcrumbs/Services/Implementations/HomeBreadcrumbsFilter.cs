using System;
using System.Linq;
using Orchard;
using Orchard.Alias;
using Orchard.Environment.Extensions;

namespace Onestop.Navigation.Breadcrumbs.Services.Implementations
{
    /// <summary>
    /// This filter appends "Home" link to the beginning of breadcrumbs path.
    /// </summary>
    [OrchardFeature("Onestop.Navigation.Breadcrumbs")]
    public class HomeBreadcrumbsFilter : IBreadcrumbsFilter
    {
        private readonly IOrchardServices _services;
        private readonly IAliasService _aliases;

        public HomeBreadcrumbsFilter(IOrchardServices services, IAliasService aliases)
        {
            _services = services;
            _aliases = aliases;
        }

        public int Priority { get { return int.MinValue; } }

        public void Apply(BreadcrumbsContext context)
        {
            if (context.Filtered) return;

            var routeValues = _aliases.Get(string.Empty);
            if (routeValues == null) return;

            object id;
            if (routeValues.TryGetValue("id", out id))
            {
                var castedId = Convert.ToInt32(id);
                var homepage = _services.ContentManager.Get(castedId);
                if (homepage != null)
                {
                    var currentHomes = context.Breadcrumbs.Segments.Where(s => (s.Content != null && s.Content.Id == homepage.Id || string.IsNullOrEmpty(s.Url.Trim('/', ' ')))).ToList();
                    if (currentHomes.Any())
                    {
                        foreach (var home in currentHomes)
                        {
                            home.DisplayText = "Home";
                        }
                    }
                    else
                    {
                        context.Breadcrumbs.Prepend(new Segment { Content = homepage, DisplayText = "Home", Url = "/" });
                    }
                }
            }
        }
    }
}