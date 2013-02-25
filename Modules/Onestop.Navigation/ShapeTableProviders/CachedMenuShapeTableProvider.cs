using Contrib.Cache.Services;
using Onestop.Navigation.Services;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;

namespace Onestop.Navigation.ShapeTableProviders
{
    public class CachedMenuShapeTableProvider : IShapeTableProvider {
        private readonly ICacheService _cache;
        private readonly ISignals _signals;
        private readonly ITokenHolder _tokenHolder;

        public CachedMenuShapeTableProvider(ICacheService cache, ISignals signals, ITokenHolder tokenHolder) {
            _cache = cache;
            _signals = signals;
            _tokenHolder = tokenHolder;
        }

        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Menu").OnDisplayed(OnDisplayed);
        }

        private void OnDisplayed(ShapeDisplayedContext displayed) {
            string key = displayed.Shape.PleaseCache;
            if (key == null) return;

            // Setting up a token just for monitoring changes from menu service.
            var menu = (IContent)displayed.Shape.Menu;
            var token = _signals.When(CacheUtility.GetCacheSignal(menu.Id));
            _tokenHolder.Set(key, token);

            var value = displayed.ChildContent;
            if (value != null)
                _cache.Put(key, value.ToString());
        }
    }
}