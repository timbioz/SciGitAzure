using Onestop.Navigation.Breadcrumbs.Models;
using Onestop.Navigation.Breadcrumbs.Services;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;

namespace Onestop.Navigation.Breadcrumbs.Drivers
{
    [OrchardFeature("Onestop.Navigation.Breadcrumbs")]
    public class BreadcrumbsWidgetPartDriver : ContentPartDriver<BreadcrumbsWidgetPart>
    {
        private readonly IBreadcrumbsService _breadcrumbs;

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public BreadcrumbsWidgetPartDriver(IBreadcrumbsService breadcrumbs)
        {
            _breadcrumbs = breadcrumbs;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        protected override DriverResult Display(BreadcrumbsWidgetPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Menu_BreadcrumbsWidget",
                () =>
                    {
                        var breadcrumbs = _breadcrumbs.Build();
                        return shapeHelper.Parts_Menu_BreadcrumbsWidget(Breadcrumbs: breadcrumbs, ContentItem: part.ContentItem, Separator: "\\");
                    });
        }
    }
}