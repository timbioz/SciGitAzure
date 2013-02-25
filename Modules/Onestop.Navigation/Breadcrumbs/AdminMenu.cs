using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;
using Orchard.Security;

namespace Onestop.Navigation.Breadcrumbs
{
    [OrchardFeature("Onestop.Navigation.Breadcrumbs")]
    public class AdminMenu : INavigationProvider
    {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder
                .Add(T("Settings"), menu => menu
                    .Add(T("Navigation"), "5", item => item
                        .Add(T("Breadcrumbs"), "2", tab => tab.Action("Index", "BreadcrumbsAdmin", new { area = "Onestop.Navigation" }).Permission(StandardPermissions.SiteOwner).LocalNav())
                    ));
        }
    }
}
