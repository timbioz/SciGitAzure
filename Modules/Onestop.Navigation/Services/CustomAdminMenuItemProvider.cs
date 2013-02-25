using System.Web;
using Onestop.Navigation.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Onestop.Navigation.Services {
    [OrchardFeature("Onestop.Navigation.AdminMenu")]
    public class CustomAdminMenuItemProvider : INavigationProvider {
        private readonly IRepository<AdminMenuItemRecord> _adminMenuItemRepository;

        public CustomAdminMenuItemProvider(IRepository<AdminMenuItemRecord> adminMenuItemRepository) {
            _adminMenuItemRepository = adminMenuItemRepository;
        }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            var menuItems = _adminMenuItemRepository.Table;
            foreach (var menuItem in menuItems) {
                var item = menuItem;
                if (string.IsNullOrWhiteSpace(item.ItemGroup)) {
                    var label = new LocalizedString(HttpUtility.HtmlEncode(item.Text));
                    builder.Add(label,
                        item.Position,
                        itemBuilder => itemBuilder.Add(label, "0", subItem => subItem.Url(item.Url)));
                }
                else {
                    builder.Add(
                        new LocalizedString(HttpUtility.HtmlEncode(item.ItemGroup)),
                        item.GroupPosition,
                        menu => menu.Add(
                            new LocalizedString(HttpUtility.HtmlEncode(item.Text)),
                            item.Position,
                            itemBuilder => itemBuilder.Url(item.Url)));
                }
            }
        }
    }
}