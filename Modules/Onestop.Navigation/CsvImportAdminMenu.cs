using System.Linq;
using Onestop.Navigation.Security;
using Onestop.Navigation.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security.Permissions;
using Orchard.UI.Navigation;
using Orchard.Utility.Extensions;

namespace Onestop.Navigation {
    [OrchardFeature("Onestop.Navigation.CsvImport")]
    public class CsvImportAdminMenu : INavigationProvider {
        private readonly IMenuService _menuServices;

        public CsvImportAdminMenu(IMenuService menuServices) {
            _menuServices = menuServices;
        }

        public string MenuName { get { return "admin"; } }
        public Localizer T { get; set; }

        public void GetNavigation(NavigationBuilder builder) {
            var menus = _menuServices.GetMenus();

            builder.AddImageSet("navigation").Add(
                T("Navigation"),
                "7",
                menu => {
                    foreach (var m in menus.OrderByDescending(c => c.As<ITitleAspect>().Title).Select(m => m.As<ITitleAspect>())) {
                        var m1 = m.As<ITitleAspect>();
                        menu.Add(T("{0}", m1.Title.CamelFriendly()), "2",
                                 item => item.Action("Index", "MenuAdmin", new { menuId = m1.Id, area = "Onestop.Navigation" })
                                             .Permission(GetPermissionVariation(Permissions.EditMenuItems, m1))
                                             .Add(T("Import items"), "4.0", 
                                                  tab => tab.Action("Index", "ImportAdmin", new { menuId = m1.Id, area = "Onestop.Navigation" })
                                                             .LocalNav()
                                                             .Permission(Permissions.CreateMenuItems)));
                    }
                });
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