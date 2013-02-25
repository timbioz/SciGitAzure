using System;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Onestop.Navigation.Utilities {
 
    /// <summary>
    /// The url utility.
    /// </summary>
    public static class UrlUtility {

        /// <summary>
        /// Determines if a given routes match.
        /// </summary>
        /// <param name="itemValues">
        /// The menu item.
        /// </param>
        /// <param name="requestValues">
        /// The route data.
        /// </param>
        /// <returns>
        /// True if the menu item's action corresponds to the route data; false otherwise.
        /// </returns>
        public static bool RouteMatches(RouteValueDictionary itemValues, RouteValueDictionary requestValues) {
            if (itemValues == null || requestValues == null) {
                return false;
            }

            if (itemValues.Keys.Any(key => requestValues.ContainsKey(key) == false)) {
                return false;
            }

            return
                itemValues.Keys.All(key =>
                    string.Equals(
                        Convert.ToString(itemValues[key]), 
                        Convert.ToString(requestValues[key]), 
                        StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Determines if given Urls match.
        /// </summary>
        /// <param name="itemHref">
        /// </param>
        /// <param name="targetUrl">
        /// </param>
        /// <param name="context">
        /// </param>
        /// <returns>
        /// The url matches.
        /// </returns>
        public static bool UrlMatches(string itemHref, string targetUrl, HttpContextBase context) {
            if (context == null) {
                return false;
            }

            if (targetUrl == null)
                return false;

            var requestUrl = targetUrl.Replace(context.Request.ApplicationPath, string.Empty).TrimEnd('/').ToUpperInvariant();
            var modelUrl = itemHref.Replace(context.Request.ApplicationPath, string.Empty).TrimEnd('/').ToUpperInvariant();

            return (!string.IsNullOrEmpty(modelUrl) && requestUrl.StartsWith(modelUrl)) || requestUrl == modelUrl;
        }
    }
}