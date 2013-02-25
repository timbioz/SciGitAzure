using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Onestop.Navigation.Models;
using Onestop.Navigation.Security;
using Onestop.Navigation.Services;
using Onestop.Navigation.ViewModels;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Onestop.Navigation.Controllers {
    [Admin, ValidateInput(false)]
    [OrchardFeature("Onestop.Navigation.CsvImport")]
    public class ImportAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _services;
        private readonly IMenuService _menuService;
        private IDictionary<string, AutoroutePart> _slugs;

        public ImportAdminController(
            IOrchardServices services,
            IMenuService menuService) {
            _services = services;
            _menuService = menuService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [HttpGet]
        public ActionResult Index(int menuId) {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null) {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(
                Permissions.CreateMenuItems,
                menu,
                T("Not allowed to create menu items for menu '{0}'", menu.As<TitlePart>().Title))) {
                return new HttpUnauthorizedResult();
            }

            var model = new CsvImportIndexViewModel { Menu = menu, Text = String.Empty };
            return View(model);
        }

        [HttpPost]
        [ActionName("Index")]
        public ActionResult IndexPOST(int menuId, string menuItems) {

            var menu = _menuService.GetMenu(menuId);
            if (menu == null) {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(
                Permissions.CreateMenuItems, 
                menu,
                T("Not allowed to create menu items for menu '{0}'", menu.As<TitlePart>().Title))) {
                return new HttpUnauthorizedResult();
            }

            var lines = menuItems.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var itemList = new List<ExtendedMenuItemPart>(lines.Count);

            _slugs = _services.ContentManager
                .Query<AutoroutePart, AutoroutePartRecord>()
                .Where(r => r.DisplayAlias != "" && r.DisplayAlias != null)
                .List()
                .ToDictionary(part => part.DisplayAlias.Trim('/', ' '), 
                              StringComparer.OrdinalIgnoreCase);

            try {
                int i = 0;
                var hasError = false;
                foreach (var line in lines) {
                    i++;
                    var menuItemData = line.Split(';')
                        .Select(s => s.Trim())
                        .ToArray();

                    if (menuItemData.Length < 3) {
                        _services.Notifier.Error(T("Error in line {0}: Incorrect parameter count '{1}'", i, line));
                        hasError = true;
                        continue;
                    }

                    IContent item;

                    // If display text is empty
                    if (string.IsNullOrWhiteSpace(menuItemData[0])) {
                        _services.Notifier.Error(T("Error in line {0}: Display text cannot be empty.", i));
                        hasError = true;
                        continue;
                    }

                    // If URL is empty
                    if (string.IsNullOrWhiteSpace(menuItemData[1])) {
                        item = _menuService.CreateMenuItem(menu.Id, "MenuItem");
                        item.As<ExtendedMenuItemPart>().DisplayHref = false;
                        item.As<MenuItemPart>().Url = "/";
                    }
                    else {
                        // Looking for matching AutoRoutePart element
                        AutoroutePart match;
                        
                        //pszmyd: Commented it out. Term items do not need a separate lookup, don't they?
                        //var matchedPath = termPathConstraint.FindPath(menuItemData[1].Trim());

                        if (!_slugs.TryGetValue(menuItemData[1].Trim('/', ' '), out match)) {
                            item = _menuService.CreateMenuItem(menu.Id, "MenuItem").As<ExtendedMenuItemPart>();
                            item.As<MenuItemPart>().Url = menuItemData[1];
                            item.As<ExtendedMenuItemPart>().Url = menuItemData[1];
                            item.As<ExtendedMenuItemPart>().DisplayHref = true;
                        }
                        else {
                            item = _menuService.CreateMenuItem(menu.Id, "ContentMenuItem").As<ContentMenuItemPart>();
                            item.As<ContentMenuItemPart>().Content = match.ContentItem;
                            item.As<ExtendedMenuItemPart>().DisplayHref = true;
                        }
                    }

                    if (menuItemData.Length > 3 && !string.IsNullOrWhiteSpace(menuItemData[3])) {
                        item.As<ExtendedMenuItemPart>().CssId = menuItemData[3];
                    }

                    if (menuItemData.Length > 4 && !string.IsNullOrWhiteSpace(menuItemData[4])) {
                        item.As<ExtendedMenuItemPart>().Classes = menuItemData[4];
                    }

                    item.As<MenuPart>().Menu = menu;
                    item.As<ExtendedMenuItemPart>().MenuVersion = null;
                    item.As<ExtendedMenuItemPart>().Text = menuItemData[0];
                    item.As<ExtendedMenuItemPart>().DisplayText = true;
                    item.As<ExtendedMenuItemPart>().Position = string.IsNullOrWhiteSpace(menuItemData[2]) ? null : menuItemData[2];

                    //_services.ContentManager.Create(item, VersionOptions.Draft);
                    itemList.Add(item.As<ExtendedMenuItemPart>());
                }

                if (hasError) {
                    throw new Exception(T("There were errors during import data processing.").Text);
                }

                foreach (var item in itemList)
                {
                    _services.ContentManager.Create(item, VersionOptions.Draft);
                }

                _services.Notifier.Information(T("Successfully imported menu items"));
                return RedirectToAction("Index", "MenuAdmin", new { menuId, area = "Onestop.Navigation" });
            }
            catch (Exception ex) {
                _services.TransactionManager.Cancel();
                _services.Notifier.Error(T("Could not import menu items. Cause: {0}.", ex.Message));
                var model = new CsvImportIndexViewModel { Menu = menu, Text = menuItems };
                return View(model);
            }
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}