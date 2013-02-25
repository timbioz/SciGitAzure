using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;
using Orchard.Environment.Extensions;

namespace Zumey.LinkRewrite
{
    [OrchardFeature("Zumey.LinkRewrite")]
    public class AdminMenu : INavigationProvider
    {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .Add(T("Settings"), menu => menu
                    .Add(T("Link Rewrite"), "8.0", subMenu => subMenu.Action("Index", "Admin", new { area = "Zumey.LinkRewrite" }).Permission(StandardPermissions.SiteOwner)
                        .Add(T("Link Rewrite"), "1.0", item => item.Action("Index", "Admin", new { area = "Zumey.LinkRewrite" }).Permission(StandardPermissions.SiteOwner).LocalNav())
                    ));

        }
    }
}
