using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Contrib.Cache.Services;
using Onestop.Navigation.Models;
using Onestop.Navigation.Services;
using Onestop.Navigation.Utilities;
using Onestop.Navigation.ViewModels;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.UI;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;
using Orchard.Widgets.Models;

namespace Onestop.Navigation.Drivers {
    public class MenuWidgetPartDriver : ContentPartDriver<OnestopMenuWidgetPart> {
        private const string TemplateName = "Parts/Menu.Widget.Edit";

        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ITokenHolder _tokenHolder;
        private readonly ICacheService _cache;
        private readonly ISignals _signals;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMenuService _menuService;
        private readonly INavigationManager _nav;
        private readonly INotifier _notifier;

        public MenuWidgetPartDriver(
            IMenuService menuService,
            INotifier notifier,
            IHttpContextAccessor httpContextAccessor,
            INavigationManager nav,
            IContentManager contentManager, 
            IWorkContextAccessor workContextAccessor, 
            ITokenHolder tokenHolder, 
            ICacheService cache, 
            ISignals signals)
        {
            _menuService = menuService;
            _notifier = notifier;
            _httpContextAccessor = httpContextAccessor;
            _nav = nav;
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
            _tokenHolder = tokenHolder;
            _cache = cache;
            _signals = signals;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        internal static MenuItem FindItemByPosition(IEnumerable<MenuItem> items, string position) {
            if (items != null && items.Any()){
                var found = items.FirstOrDefault(i => i.Position == position);
                return found ?? items.Select(item => FindItemByPosition(item.Items, position)).FirstOrDefault(iFound => iFound != null);
            }

            return null;
        }

        protected static bool CollapseNonSiblings(IEnumerable<MenuItem> menuItems, MenuItem item) {
            var retVal = false;
            if (menuItems != null) {
                foreach (var menuItem in menuItems) {
                    if (menuItem == item) {
                        retVal = true;
                    }
                    else {
                        var isSelected = CollapseNonSiblings(menuItem.Items, item);
                        if (!isSelected) {
                            menuItem.Items = null;
                        }
                        else{
                            retVal = true;
                        }
                    }
                }
            }

            return retVal;
        }

        protected dynamic BuildMenuItemShape(dynamic shapeFactory, dynamic parentShape, dynamic menu, MenuItem menuItem, OnestopMenuWidgetPart settings, int level) {
            var item = shapeFactory.MenuItem()
                .Text(menuItem.Text)
                .IdHint(menuItem.IdHint)
                .Href(menuItem.Href)
                .LinkToFirstChild(menuItem.LinkToFirstChild)
                .LocalNav(menuItem.LocalNav)
                .Selected(menuItem.Selected)
                .RouteValues(menuItem.RouteValues)
                .Item(menuItem)
                .Menu(menu)
                .Parent(parentShape)
                .Content(menuItem.Content)
                .WrapChildrenInDiv(settings.WrapChildrenInDiv)
                .Level(level);

            if (menuItem.Content != null && menuItem.Content.Is<ExtendedMenuItemPart>()) {
                var part = menuItem.Content.As<ExtendedMenuItemPart>();

                item.DisplayHref(part.DisplayHref)
                    .DisplayText(part.DisplayText)
                    .SubTitle(part.SubTitle)
                    .Group(part.GroupName)
                    .InNewWindow(part.InNewWindow);

                if (!string.IsNullOrWhiteSpace(part.Classes)) {
                    foreach (var c in part.Classes.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)){
                        item.Classes.Add(c.Trim());
                    }
                }

                if (!string.IsNullOrWhiteSpace(part.CssId)) {
                    item.Id = part.CssId.Trim();
                }
            }

            var currentContext = _httpContextAccessor.Current();

            /* Setting currently selected item */
            item.Current(currentContext != null &&
                (UrlUtility.RouteMatches(menuItem.RouteValues, currentContext.Request.RequestContext.RouteData.Values) ||
                UrlUtility.UrlMatches(menuItem.Href, currentContext.Request.Path, currentContext)));

            return item;
        }

        protected override DriverResult Display(OnestopMenuWidgetPart part, string displayType, dynamic shapeHelper) {
            // Do not try to render the menu widget when in Dashboard
            if (AdminFilter.IsApplied(_workContextAccessor.GetContext().HttpContext.Request.RequestContext)) return null;

            switch (displayType) {
                case "Detail":
                    return ContentShape("Parts_Menu_Widget", () => BuildDisplayShape(part, shapeHelper));
                default:
                    return null;
            }
        }

        private dynamic BuildDisplayShape(OnestopMenuWidgetPart part, dynamic shapeHelper) {
            if (part.Menu == null) {
                return null;
            }

            var request = _httpContextAccessor.Current().Request;
            var requestData = request.RequestContext.RouteData;
            var requestDictionary = requestData.Values.Where(val => val.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // Adding query string parameters to dictionary
            foreach (string key in request.QueryString.Keys) {
                if (key == null) continue;
                if (!requestDictionary.ContainsKey(key) && request.QueryString[key] != null){
                    requestDictionary[key] = request.QueryString[key];
                }
            }

            var isCalledByName = requestDictionary.ContainsKey("menuItemName");
            var currentContext = _httpContextAccessor.Current();
            var currentCulture = _workContextAccessor.GetContext().CurrentCulture;

            var shapeKey = string.Format("Shape::OnestopMenuWidget::{8}_{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}___{9}",
                part.Menu.Id,
                part.Mode,
                part.Levels,
                part.RootNode,
                part.WrapChildrenInDiv,
                part.CutOrFlattenLower,
                isCalledByName ? requestDictionary["menuItemName"] : "",
                currentCulture,
                part.As<WidgetPart>().Zone,
                currentContext.Request.Path.Trim('/', ' ').ToLowerInvariant());

            // OUTPUT CACHE VALIDITY CHECK
            //----------------------------------------------------------------------------
            IVolatileToken cacheToken;
            var menuShapeVal = _cache.Get<string>(shapeKey);
            dynamic menuShape = new HtmlString(menuShapeVal);
            bool shouldRefreshCache = menuShape == null || !_tokenHolder.TryGet(shapeKey, out cacheToken) || !cacheToken.IsCurrent;
            //--------------------------------------------------------------------------

            if (shouldRefreshCache) {
                var menu = _menuService.GetMenu(part.Menu.Id);

                if (menu == null) {
                    return null;
                }

                var itemsDictionary = GetItemsDictionary(part, requestDictionary);
                menuShape = shapeHelper.Menu()
                        .Menu(menu)
                        .MenuName(menu.As<ITitleAspect>().Title.HtmlClassify())
                        .ZoneName(part.As<WidgetPart>().Zone.HtmlClassify())
                        .ItemId(part.ContentItem.Id.ToString(CultureInfo.InvariantCulture))
                        .PleaseCache(shapeKey);

                var shapes = itemsDictionary.Select(pair => {
                    var level = pair.Key.Split('.', ',').Length;
                    return new KeyValuePair<string, dynamic>(
                        pair.Key, BuildMenuItemShape(
                            shapeHelper,
                            null,
                            menuShape,
                            pair.Value,
                            part,
                            level));
                }).ToDictionary(kv => kv.Key, kv => kv.Value);

                // Set up parent-child relationships
                // We have to do it after populating the whole shape dictionary.
                foreach (var pair in shapes.OrderBy(kv => kv.Key, new FlatPositionComparer())) {
                    var splitted = pair.Key.Split('.', ',');

                    dynamic parent;
                    if (shapes.TryGetValue(string.Join(".", splitted.Take(splitted.Length - 1)), out parent)) {
                        pair.Value.Parent(parent);
                        // Due to ordering, parents will always come before it's children
                        // so they already have their level corrected.
                        pair.Value.Level(parent.Level + 1);
                        parent.Add(pair.Value, pair.Key);
                    }
                    else if (splitted.Length == 1)
                    {
                        // Due to ordering, parents will always come before it's children
                        pair.Value.Level(1);
                        pair.Value.Parent(menuShape);
                        menuShape.Add(pair.Value, pair.Key);
                    }
                }
            }

            var partShape = shapeHelper.Parts_Menu_Widget().Add(menuShape);
            return partShape;

        }

        private static void PopulateDictionary(IEnumerable<MenuItem> items, ConcurrentDictionary<string, MenuItem> currentDict, Func<string, bool> positionSelector) {
            if (items == null) return;

            foreach (var item in items) {
                if (!positionSelector(item.Position)) return;
                var itemLocal = item;
                currentDict.AddOrUpdate(item.Position, item, (s, menuItem) => (itemLocal));
                PopulateDictionary(item.Items, currentDict, positionSelector);
            }
        }

        protected override DriverResult Editor(OnestopMenuWidgetPart part, dynamic shapeHelper) {
            var model = new MenuWidgetViewModel {
                CurrentMenuId = part.Menu == null ? -1 : part.Menu.Id,
                CutOrFlattenLower = part.CutOrFlattenLower,
                Levels = part.Levels,
                Mode = part.Mode,
                RootNode = part.RootNode,
                WrapChildrenInDivs = part.WrapChildrenInDiv,
                Menus = _menuService.GetMenus(),
                Modes = Enum.GetValues(typeof(MenuWidgetMode)).Cast<MenuWidgetMode>()
            };

            return ContentShape("Parts_Menu_Widget", () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }


        protected override DriverResult Editor(OnestopMenuWidgetPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new MenuWidgetViewModel();

            if (updater.TryUpdateModel(model, Prefix, null, null)) {

                // Clearing old menu cache
                if (part.Menu != null) {
                    _signals.Trigger(CacheUtility.GetCacheSignal(part.Menu.Id));
                }

                part.CutOrFlattenLower = model.CutOrFlattenLower;
                part.Levels = model.Levels;
                part.Mode = model.Mode;
                part.RootNode = model.RootNode;
                part.WrapChildrenInDiv = model.WrapChildrenInDivs;
                part.Menu = _contentManager.Get(model.CurrentMenuId).Record;

                // Clearing new menu cache
                if (part.Menu != null) {
                    _signals.Trigger(CacheUtility.GetCacheSignal(part.Menu.Id));
                }

                _notifier.Information(T("Menu widget edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during menu widget update!"));
            }

            return Editor(part, shapeHelper);
        }

        private IDictionary<string, MenuItem> GetItemsDictionary(OnestopMenuWidgetPart part, IDictionary<string, object> requestDictionary) {
            var request = _httpContextAccessor.Current().Request;
            var requestData = request.RequestContext.RouteData;
            var isCalledByName = requestDictionary.ContainsKey("menuItemName");
            var menuItems = _nav.BuildMenu(_contentManager.Get(part.Menu.Id));
            var currentCulture = _workContextAccessor.GetContext().CurrentCulture;

            var localized = new List<MenuItem>();
            foreach (var menuItem in menuItems) {
                // if there is no associated content, treat it as culture neutral
                if (menuItem.Content == null) {
                    localized.Add(menuItem);
                }

                // if the menu item is culture neutral or of the current culture
                else if (String.IsNullOrEmpty(menuItem.Culture) || String.Equals(menuItem.Culture, currentCulture, StringComparison.OrdinalIgnoreCase)) {
                    localized.Add(menuItem);
                }
            }

            menuItems = localized;

            // Set the currently selected path
            MenuItemsUtility.SetSelectedPath(
                menuItems,
                requestData,
                _httpContextAccessor.Current().Request.Path,
                _httpContextAccessor.Current());

            if (!string.IsNullOrWhiteSpace(part.RootNode)) {
                var item = FindItemByPosition(menuItems, part.RootNode);
                if (item != null) {
                    menuItems = item.Items.OrderBy(m => m.Position, new FlatPositionComparer()).ToList();
                }
            }

            switch (part.Mode) {
                case MenuWidgetMode.AllItems:
                    break;
                case MenuWidgetMode.ChildrenOnly:
                    var item = isCalledByName ? MenuItemsUtility.GetItemByName(menuItems, requestDictionary["menuItemName"].ToString())
                                              : MenuItemsUtility.GetItemByUrl(menuItems, requestData, request.Path, _httpContextAccessor.Current());
                    menuItems = item != null ? item.Items.OrderBy(m => m.Position, new FlatPositionComparer()).ToList()
                                         : Enumerable.Empty<MenuItem>();
                    break;
                case MenuWidgetMode.SiblingsOnly:
                    var childItem = isCalledByName ? MenuItemsUtility.GetItemByName(menuItems, requestDictionary["menuItemName"].ToString())
                                                   : MenuItemsUtility.GetItemByUrl(menuItems, requestData, request.Path, _httpContextAccessor.Current());
                    var parent = MenuItemsUtility.GetParent(menuItems, childItem);
                    if (parent != null) {
                        menuItems = parent.Items.OrderBy(m => m.Position, new FlatPositionComparer()).ToList();
                    }

                    break;
                case MenuWidgetMode.SiblingsExpanded:
                    // Got to make a copy so children collapsing would not affect the cached items.
                    CollapseNonSiblings(
                        menuItems,
                        isCalledByName ? MenuItemsUtility.GetItemByName(menuItems, requestDictionary["menuItemName"].ToString())
                                       : MenuItemsUtility.GetItemByUrl(menuItems, requestData, request.Path, _httpContextAccessor.Current()));
                    break;
            }

            // Unfolding a built menu to a flat item dictionary
            var dict = new ConcurrentDictionary<string, MenuItem>();
            PopulateDictionary(menuItems, dict, position => {
                                                var level = position.Split('.', ',').Length;
                                                return !(part.Levels > 0 && level > part.Levels);
                                            });

            return dict;
        }
    }
}