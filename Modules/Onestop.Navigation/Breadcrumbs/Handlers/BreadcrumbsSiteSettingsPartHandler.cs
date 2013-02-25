using JetBrains.Annotations;
using Onestop.Navigation.Breadcrumbs.Models;
using Onestop.Navigation.Breadcrumbs.Services.Implementations;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Onestop.Navigation.Breadcrumbs.Handlers
{
    [UsedImplicitly]
    [OrchardFeature("Onestop.Navigation.Breadcrumbs")]
    public class BreadcrumbsSiteSettingsPartHandler : ContentHandler
    {
        public BreadcrumbsSiteSettingsPartHandler(IRepository<BreadcrumbsSiteSettingsPartRecord> repository) {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<BreadcrumbsSiteSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));

            OnLoaded<BreadcrumbsSiteSettingsPart>((ctx, part) => {
                if (string.IsNullOrWhiteSpace(part.DefaultProvider)) {
                    part.DefaultProvider = ContainerBreadcrumbsProvider.ProviderName;
                }
            });
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Navigation")));
        }
    }
}