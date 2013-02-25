using System.Linq;
using System.Web;
using Onestop.Navigation.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Contents;
using Orchard.Core.Navigation.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security.Permissions;
using Orchard.UI;
using Orchard.UI.Navigation;

namespace Onestop.Navigation.Services {
    [OrchardSuppressDependency("Orchard.Core.Navigation.Services.DefaultMenuProvider")]
    public class OnestopMenuProvider : IMenuProvider {
        private readonly IContentManager _contentManager;
        private readonly IMenuService _menuService;

        public OnestopMenuProvider(IContentManager contentManager, IMenuService menuService) {
            _contentManager = contentManager;
            _menuService = menuService;
        }

        public void GetMenu(IContent menu, NavigationBuilder builder)
        {
            var menuItems = _menuService.GetMenuItems(menu).OrderBy(i => i.As<ExtendedMenuItemPart>().Position, new FlatPositionComparer());

            foreach (var menuPart in menuItems) {
                if (menuPart != null) {
                    var part = menuPart;

                    // fetch the culture of the menu item, if any
                    string culture = null;
                    var localized = part.As<ILocalizableAspect>();
                    if(localized != null) {
                        culture = localized.Culture;
                    }

                    var permission = !string.IsNullOrWhiteSpace(part.As<ExtendedMenuItemPart>().Permission)
                         ? Permission.Named(part.As<ExtendedMenuItemPart>().Permission)
                         : Permissions.ViewContent;

                    var technicalName = part.As<ExtendedMenuItemPart>().TechnicalName;

                    if (part.Is<MenuItemPart>())
                        builder.Add(
                            new LocalizedString(HttpUtility.HtmlEncode(part.As<ExtendedMenuItemPart>().Text)), 
                            part.As<ExtendedMenuItemPart>().Position, 
                            item => item.Url(part.As<MenuItemPart>().Url)
                                        .Content(part)
                                        .Culture(culture)
                                        .Permission(permission)
                                        .IdHint(technicalName));
                    else
                        builder.Add(
                            new LocalizedString(HttpUtility.HtmlEncode(part.As<ExtendedMenuItemPart>().Text)), 
                            part.As<ExtendedMenuItemPart>().Position,
                            item => item.Action(_contentManager.GetItemMetadata(part.ContentItem).DisplayRouteValues)
                                        .Content(part)
                                        .Culture(culture)
                                        .Permission(permission)
                                        .IdHint(technicalName));

                }
            }
        }
    }
}