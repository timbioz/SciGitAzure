using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;
using Onestop.Navigation.Security;
using Onestop.Navigation.Services;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.UI.Navigation;
using Orchard.Utility.Extensions;

namespace Onestop.Navigation {
    [OrchardSuppressDependency("Orchard.Core.Navigation.AdminMenu")]
    public class NavigationAdminMenu : INavigationProvider {
        private readonly IMenuService _menuServices;

        public NavigationAdminMenu(IMenuService menuServices) {
            _menuServices = menuServices;
        }

        public string MenuName { get { return "admin"; } }
        public Localizer T { get; set; }

        public void GetNavigation(NavigationBuilder builder) {
            var menus = _menuServices.GetMenus();

            builder
                .AddImageSet("navigation")
                .Add(T("Navigation"), "7",
                    menu => {
                        menu.Add(T("Create new menu"), "1",
                            item => item
                                .Action("Create", "Admin", new { area = "Contents", id = "Menu" })
                                .Permission(Orchard.Core.Navigation.Permissions.ManageMainMenu));
                        foreach (var m in menus.OrderByDescending(c => c.As<ITitleAspect>().Title)){
                            var m1 = m.As<ITitleAspect>();
                            menu.Add(T("{0}", m1.Title.CamelFriendly()), "2",
                                     item => {
                                         item.Action("Index", "MenuAdmin", new { menuId = m1.Id, area = "Onestop.Navigation" }).Permission(GetPermissionVariation(Permissions.EditMenuItems, m1))
                                             .Add(T("Manage menu"), "1.0", tab => tab.Action("Index", "MenuAdmin", new { menuId = m1.Id, area = "Onestop.Navigation" }).LocalNav().Permission(StandardPermissions.SiteOwner))
                                             .Add(T("Removed items"), "2.0", tab => tab.Action("Removed", "MenuAdmin", new { menuId = m1.Id, area = "Onestop.Navigation" }).LocalNav().Permission(StandardPermissions.SiteOwner))
                                             .Add(T("History"), "3.0", tab => tab.Action("History", "MenuAdmin", new { menuId = m1.Id, area = "Onestop.Navigation" }).LocalNav().Permission(StandardPermissions.SiteOwner));

                                         foreach (var version in m1.ContentItem.Record.Versions) {
                                             ContentItemVersionRecord version1 = version;
                                             item.Add(preview =>
                                                 preview.Action("Preview", "MenuAdmin", new { menuId = m1.Id, versionNumber = version1.Number, area = "Onestop.Navigation" }).Permission(GetPermissionVariation(Permissions.EditMenuItems, m1)).LocalNav()
                                                        .Add(local => local.Add(T("Version preview"), "1.0", tab => tab.Action("Preview", "MenuAdmin", new { menuId = m1.Id, versionNumber = version1.Number, area = "Onestop.Navigation" }).LocalNav().Permission(StandardPermissions.SiteOwner))));
                                         }
                                     });
                        }
                    });

            // Site-level settings (need to explicitly add that in order to add more tabs).
            builder
                .Add(T("Settings"), menu => menu
                    .Add(T("Navigation"), "5", subMenu => subMenu.Action("Index", "Admin", new { area = "Settings", groupInfoId = "Navigation" }).Permission(StandardPermissions.SiteOwner)
                        .Add(T("Global settings"), "1", item => item.Action("Index", "Admin", new { area = "Settings", groupInfoId = "Navigation" }).Permission(StandardPermissions.SiteOwner).LocalNav())
                    ));
        }

        /// <summary>
        /// Gets the given permission variation (if any) for the given menu.
        /// </summary>
        /// <param name="permission">Permission to look variation for.</param>
        /// <param name="menu">Menu to look variation for.</param>
        /// <returns>Appropriate permission.</returns>
        private static Permission GetPermissionVariation(Permission permission, IContent menu) {
            var variation = MenuPermissions.ConvertToDynamicPermission(permission);

            if (variation == null) {
                return permission;
            }

            variation = MenuPermissions.CreateDynamicPermission(variation, menu);
            return variation;
        }
    }
}