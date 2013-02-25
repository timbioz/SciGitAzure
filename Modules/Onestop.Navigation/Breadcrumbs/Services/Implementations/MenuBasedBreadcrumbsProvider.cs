using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Onestop.Navigation.Utilities;
using Onestop.Patterns.Services;
using Orchard;
using Orchard.Alias;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Navigation.Services;
using Orchard.Environment.Extensions;
using Orchard.UI.Navigation;

namespace Onestop.Navigation.Breadcrumbs.Services.Implementations {
    /// <summary>
    /// Menu based providers. Builds breadcrumbs based on a given menu.
    /// </summary>
    [OrchardFeature("Onestop.Navigation.Breadcrumbs")]
    public class MenuBasedBreadcrumbsProvider : Component, IBreadcrumbsProvider {
        private readonly IMenuService _menus;
        private readonly INavigationManager _manager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IAliasService _aliases;
        private readonly IPatternService _patterns;

        private const string Prefix = "Menu::";

        public MenuBasedBreadcrumbsProvider(
            IMenuService menus,
            INavigationManager manager, 
            IWorkContextAccessor workContextAccessor, 
            IAliasService aliases, 
            IPatternService patterns) {
            _menus = menus;
            _manager = manager;
            _workContextAccessor = workContextAccessor;
            _aliases = aliases;
            _patterns = patterns;
            }

        public int Priority { get { return 0; } }

        public IEnumerable<BreadcrumbsProviderDescriptor> Descriptors {
            get {
                return _menus.GetMenus().Select(m => new BreadcrumbsProviderDescriptor {
                    Name = Prefix + m.Id,
                    DisplayText = T("Menu provider based on '{0}'.", m.As<ITitleAspect>().Title).Text,
                });
            }
        }

        public bool Match(BreadcrumbsContext context)
        {
            return Descriptors.Any(d => d.Name.Equals(context.Provider, StringComparison.OrdinalIgnoreCase));
        }

        public void Build(BreadcrumbsContext context)
        {
            var menuId = Convert.ToInt32(context.Provider.Substring(Prefix.Length));
            if (context.Content != null) {
                var itemPath = context.Content.As<IAliasAspect>().Path;
                var itemValues = _aliases.Get(itemPath);

                context.Paths = new[] { itemPath }.Concat(context.Paths.ToList());

                if (itemValues != null) {
                    context.RouteValues = new[] { itemValues }.Concat(context.RouteValues.ToList());   
                }
            }

            var breadcrumbs = GetSources(FindMatch(context, menuId));
            context.Breadcrumbs = breadcrumbs;
        }

        private IEnumerable<MenuItem> FindMatch(BreadcrumbsContext context, int menuId)
        {
            var menuItems = _manager.BuildMenu(_menus.GetMenu(menuId));
            var workContext = _workContextAccessor.GetContext();
            var currentCulture = workContext.CurrentCulture;

            var localized = new List<MenuItem>();
            foreach (var menuItem in menuItems)
            {
                // if there is no associated content, it as culture neutral
                if (menuItem.Content == null)
                {
                    localized.Add(menuItem);
                }

                // if the menu item is culture neutral or of the current culture
                else if (String.IsNullOrEmpty(menuItem.Culture) || String.Equals(menuItem.Culture, currentCulture, StringComparison.OrdinalIgnoreCase))
                {
                    localized.Add(menuItem);
                }
            }

            object lookupPattern;

            // Find first match
            IEnumerable<MenuItem> selectedPath = null;
            var predicate = context.Properties.TryGetValue("MenuLookupPattern", out lookupPattern)
                                ? (Func<string, RouteValueDictionary, bool>)((path, values) => _patterns.TryMatch(path, lookupPattern.ToString()))
                                : null;

            foreach (var path in context.Paths)
            {
                selectedPath = MenuItemsUtility.SetSelectedPath(localized, null, path, workContext.HttpContext, predicate);
                if (selectedPath != null) break;
            }

            if (selectedPath == null)
            {
                foreach (var value in context.RouteValues)
                {
                    selectedPath = MenuItemsUtility.SetSelectedPath(localized, value, null, workContext.HttpContext, null);
                    if (selectedPath != null) break;
                }
            }

            return selectedPath ?? Enumerable.Empty<MenuItem>();
        }

        private Breadcrumbs GetSources(IEnumerable<MenuItem> selectedPath)
        {
            var breadcrumbs = new Breadcrumbs();

            if (selectedPath != null) {
                foreach (var menuItem in selectedPath) {
                    breadcrumbs.Append(new Segment { Content = menuItem.Content, DisplayText = menuItem.Text.Text, Url = menuItem.Href });
                }
            }

            return breadcrumbs;
        }
    }
}