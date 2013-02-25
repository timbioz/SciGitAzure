using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Caching;
using Zumey.LinkRewrite.Models;
using Zumey.LinkRewrite.Services;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Zumey.LinkRewrite.Handlers
{
    [OrchardFeature("Zumey.LinkRewrite")]
    public class LinkRewriteSettingsHandler : ContentHandler
    {

        public LinkRewriteSettingsHandler(IRepository<LinkRewriteSettingsRecord> repository)
        {
            Filters.Add(new ActivatingFilter<LinkRewriteSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
            OnInitializing<LinkRewriteSettingsPart>((context, part) =>
            {
                part.Enabled = false;
                part.Rules = string.Empty;
            });
        }


    }

}
