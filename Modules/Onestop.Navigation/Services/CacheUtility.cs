namespace Onestop.Navigation.Services
{
    /// <summary>
    /// A small utility class for usage in menu caching scenarios.
    /// </summary>
    public static class CacheUtility
    {
        public const string MenuItemsCacheSignal = "Onestop.Navigation.MenuItems";
        public static string GetCacheSignal(int menuId)
        {
            return MenuItemsCacheSignal + "." + menuId;
        }
    }
}