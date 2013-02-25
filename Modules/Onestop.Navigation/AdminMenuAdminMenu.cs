using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Onestop.Navigation
{
    [OrchardFeature("Onestop.Navigation.AdminMenu")]
    public class AdminMenuAdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }
        public Localizer T { get; set; }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("navigation").Add(
                T("Navigation"),
                "7",
                menu => menu.Add(T("Admin Menu"), "3", 
                    item => item.Action("Index", "AdminNavigationAdmin", new { area = "Onestop.Navigation" })
                                .Permission(Orchard.Core.Navigation.Permissions.ManageMainMenu)));
        }
    }
}