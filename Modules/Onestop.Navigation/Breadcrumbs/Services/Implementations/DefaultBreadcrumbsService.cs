using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Onestop.Navigation.Breadcrumbs.Models;
using Orchard;
using Orchard.Alias;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Html;
using Orchard.Tokens;

namespace Onestop.Navigation.Breadcrumbs.Services.Implementations
{
    [OrchardFeature("Onestop.Navigation.Breadcrumbs")]
    public class DefaultBreadcrumbsService : IBreadcrumbsService
    {
        private readonly IEnumerable<IBreadcrumbsProvider> _providers;
        private readonly IEnumerable<IBreadcrumbsFilter> _filters;
        private readonly UrlHelper _urlHelper;
        private readonly ITokenizer _tokenizer;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IContentManager _contentManager;
        private readonly IAliasService _aliases;
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<RoutePatternRecord> _repository;
        private readonly ISignals _signals;

        public const string PatternsCacheKey = "Onestop.Navigation.Breadcrumbs.Patterns";
        public const string ProvidersCacheKey = "Onestop.Navigation.Breadcrumbs.Providers";

        public DefaultBreadcrumbsService(
            IEnumerable<IBreadcrumbsProvider> providers,
            IEnumerable<IBreadcrumbsFilter> filters,
            UrlHelper urlHelper,
            ITokenizer tokenizer,
            IWorkContextAccessor workContextAccessor,
            IContentManager contentManager,
            IAliasService aliases,
            ICacheManager cacheManager,
            IRepository<RoutePatternRecord> repository, 
            ISignals signals)
        {
            _providers = providers;
            _filters = filters;
            _urlHelper = urlHelper;
            _tokenizer = tokenizer;
            _workContextAccessor = workContextAccessor;
            _contentManager = contentManager;
            _aliases = aliases;
            _cacheManager = cacheManager;
            _repository = repository;
            _signals = signals;
        }

        public Breadcrumbs Build(IContent item = null, RouteValueDictionary routeData = null, string path = null)
        {
            var content = item ?? GetCurrentContent();
            var request = _workContextAccessor.GetContext().HttpContext.Request;

            var context = new BreadcrumbsContext
            {
                Content = content,
                RouteValues = new[]{ routeData ?? request.RequestContext.RouteData.Values },
                Paths = new[]{ path ?? request.Path }
            };

            var provider = GetProviders().FirstOrDefault(p => p.Match(context))
                ?? GetProviders().First(p => p.Descriptors.Any(desc => desc.Name == GetDefaultProvider().Name));

            provider.Build(context);

            // Pushing context to breadcrumbs object so shapes can make use of it
            context.Breadcrumbs.Context = context;
            return Finish(Filter(Fill(context))).Breadcrumbs;
        }

        public BreadcrumbsProviderDescriptor GetDefaultProvider()
        {
            var siteSettings = _workContextAccessor.GetContext().CurrentSite.As<BreadcrumbsSiteSettingsPart>();
            return GetProviderDescriptors().FirstOrDefault(d => d.Name == siteSettings.DefaultProvider)
                ?? GetProviderDescriptors().FirstOrDefault(d => d.Name == "Default");
        }

        public IEnumerable<RoutePattern> GetPatterns()
        {
            return _cacheManager.Get(PatternsCacheKey,
                ctx => {
                        ctx.Monitor(_signals.When(PatternsCacheKey));
                        var defaultProvider = GetDefaultProvider();
                        return _repository.Table.OrderByDescending(p => p.Priority)
                            .Select(pattern =>
                                new RoutePattern
                                {
                                    Id = pattern.Id,
                                    Pattern = pattern.Pattern,
                                    Priority = pattern.Priority,
                                    Provider = pattern.Provider,
                                    Editable = true,
                                    Global = false
                                })
                            .ToList()
                            .Concat(new[] { 
                                new RoutePattern 
                                {
                                    Pattern = "{all*}", 
                                    Priority = int.MinValue, 
                                    Editable = false, 
                                    Provider = defaultProvider.Name,
                                    Global = true
                                }})
                            .ToList();
                });
        }

        public void AddPattern(string pattern, string provider)
        {
            var currentMax = _repository.Table.Max(p => (int?)p.Priority);
            var record = new RoutePatternRecord
            {
                Pattern = pattern,
                Provider = provider,
                Priority = currentMax.HasValue ? currentMax.Value + 1 : 1
            };

            _repository.Create(record);
            _signals.Trigger(PatternsCacheKey);
        }

        public void DeletePattern(int id)
        {
            var record = _repository.Get(id);

            if (record == null)
            {
                return;
            }

            _repository.Delete(record);
            _signals.Trigger(PatternsCacheKey);
        }


        public IEnumerable<IBreadcrumbsProvider> GetProviders()
        {
            return _providers.OrderByDescending(p => p.Priority);
        }

        public IEnumerable<BreadcrumbsProviderDescriptor> GetProviderDescriptors()
        {
            return _cacheManager.Get(ProvidersCacheKey, ctx => GetProviders().SelectMany(p => p.Descriptors).ToList());
        }

        private BreadcrumbsContext Fill(BreadcrumbsContext context)
        {
            var i = 0;
            foreach (var segment in context.Breadcrumbs.Segments)
            {
                segment.Url = string.IsNullOrWhiteSpace(segment.Url) ? GetUrl(segment.Content) : segment.Url;
                segment.Index = i;
                i++;
            }

            return context;
        }

        private BreadcrumbsContext Filter(BreadcrumbsContext context)
        {
            // Applying post-build filters
            foreach (var filter in _filters.OrderByDescending(f => f.Priority))
            {
                filter.Apply(context);
            }

            return context;
        }

        private BreadcrumbsContext Finish(BreadcrumbsContext context)
        {
            foreach (var segment in context.Breadcrumbs.Segments)
            {
                segment.DisplayText = _tokenizer.Replace(segment.DisplayText, BuildTokenContext(segment.Content), new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });
                segment.Url = _tokenizer.Replace(segment.Url, BuildTokenContext(segment.Content), new ReplaceOptions { Encoding = ReplaceOptions.UrlEncode });
            }

            return context;
        }

        private IDictionary<string, object> BuildTokenContext(IContent item)
        {
            return new Dictionary<string, object> { { "Content", item } };
        }

        public string GetUrl(IContent content)
        {
            return content == null ? null : _urlHelper.ItemDisplayUrl(content);
        }

        private IContent GetCurrentContent()
        {
            var context = _workContextAccessor.GetContext();
            if (context == null || context.HttpContext == null) return null;

            // Checking if the page we're currently on is a content item
            var routeValues = _aliases.Get(context.HttpContext.Request.AppRelativeCurrentExecutionFilePath.Substring(1).Trim('/'));
            if (routeValues == null) return null;

            object id;
            if (routeValues.TryGetValue("id", out id))
            {
                var castedId = Convert.ToInt32(id);
                return _contentManager.Get(castedId);
            }

            return null;
        }
    }
}