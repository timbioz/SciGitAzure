using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Orchard.UI.Navigation;

namespace Onestop.Navigation.Utilities
{
    public static class MenuItemsUtility
    {
        public static MenuItem GetItemByUrl(IEnumerable<MenuItem> menuItems, RouteData currentRouteData, string targetUrl, HttpContextBase httpContext) {
            if (menuItems == null) {
                return null;
            }

            foreach (var menuItem in menuItems) {
                var item = GetItemByUrl(menuItem.Items, currentRouteData, targetUrl, httpContext);
                if (item != null) {
                    return item;
                }

                if (UrlUtility.RouteMatches(menuItem.RouteValues, currentRouteData.Values)
                    || UrlUtility.UrlMatches(menuItem.Href, targetUrl, httpContext)) {
                    return menuItem;
                }
            }

            return null;
        }

        public static MenuItem GetItemByName(IEnumerable<MenuItem> menuItems, string name) {
            var tokens = name.ToLowerInvariant().Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (menuItems == null || tokens.Length == 0) {
                return null;
            }

            foreach (var menuItem in menuItems) {
                if (HttpUtility.HtmlDecode(menuItem.Text.Text).ToLowerInvariant().Trim() != tokens[0]) {
                    continue;
                }

                var foundItem = menuItem;
                if (tokens.Length > 0) {
                    var item = GetItemByName(menuItem.Items, string.Join("/", tokens.Skip(1)));
                    if (item != null) {
                        foundItem = item;
                    }
                }

                return foundItem;
            }

            return null;
        }

        public static MenuItem GetParent(IEnumerable<MenuItem> menuItems, MenuItem child, MenuItem currentParent = null) {
            if (menuItems == null || child == null) {
                return null;
            }

            foreach (var menuItem in menuItems) {
                if (menuItem == child) {
                    return currentParent;
                }

                var item = GetParent(menuItem.Items, child, menuItem);
                if (item != null) {
                    return item;
                }
            }

            return null;
        }

        public static Stack<MenuItem> SetSelectedPath(IEnumerable<MenuItem> menuItems, RouteData currentRouteData, string targetUrl, HttpContextBase httpContext)
        {
            return SetSelectedPath(menuItems, currentRouteData.Values, targetUrl, httpContext, null);
        }

        public static Stack<MenuItem> SetSelectedPath(IEnumerable<MenuItem> menuItems, RouteValueDictionary currentRouteData, string targetUrl, HttpContextBase httpContext, Func<string, RouteValueDictionary, bool> predicate)
        {
            if (predicate == null) {
                predicate = (path, route) => false;
            }
            
            if (menuItems == null) {
                return null;
            }

            foreach (var menuItem in menuItems) {
                var selectedPath = SetSelectedPath(menuItem.Items, currentRouteData, targetUrl, httpContext, predicate);
                if (selectedPath != null) {
                    menuItem.Selected = true;
                    selectedPath.Push(menuItem);
                    return selectedPath;
                }

                if (UrlUtility.RouteMatches(menuItem.RouteValues, currentRouteData) ||
                    UrlUtility.UrlMatches(menuItem.Href, targetUrl, httpContext) || 
                    predicate(menuItem.Href, menuItem.RouteValues))
                {
                    menuItem.Selected = true;
                    selectedPath = new Stack<MenuItem>();
                    selectedPath.Push(menuItem);
                    return selectedPath;
                }
            }

            return null;
        }
    }
}