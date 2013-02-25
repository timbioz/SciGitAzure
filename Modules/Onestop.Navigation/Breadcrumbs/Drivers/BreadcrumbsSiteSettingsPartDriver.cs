using JetBrains.Annotations;
using Onestop.Navigation.Breadcrumbs.Models;
using Onestop.Navigation.Breadcrumbs.Services;
using Onestop.Navigation.Breadcrumbs.Services.Implementations;
using Onestop.Navigation.Breadcrumbs.ViewModels;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Localization;

namespace Onestop.Navigation.Breadcrumbs.Drivers
{
    [UsedImplicitly]
    [OrchardFeature("Onestop.Navigation.Breadcrumbs")]
    public class BreadcrumbsSiteSettingsPartDriver : ContentPartDriver<BreadcrumbsSiteSettingsPart>
    {
        private const string TemplateName = "Parts/Menu.BreadcrumbsSiteSettings.Edit";

        private readonly IBreadcrumbsService _service;
        private readonly ISignals _signals;

        public BreadcrumbsSiteSettingsPartDriver(
            IBreadcrumbsService service, 
            ISignals signals)
        {
            _service = service;
            _signals = signals;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        protected override string Prefix { get { return "BreadcrumbsSiteSettings"; } }

        protected override DriverResult Editor(BreadcrumbsSiteSettingsPart part, dynamic shapeHelper)
        {
            var model = new BreadcrumbsSettingsViewModel
            {
                DefaultProvider = part.DefaultProvider,
                Providers = _service.GetProviderDescriptors()
            };

            return ContentShape("Parts_Menu_BreadcrumbsSiteSettings_Edit", () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix))
                       .OnGroup("navigation");
        }

        protected override DriverResult Editor(BreadcrumbsSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            var model = new BreadcrumbsSettingsViewModel
            {
                DefaultProvider = part.DefaultProvider,
            };

            if (updater.TryUpdateModel(model, Prefix, null, null))
            {
                part.DefaultProvider = model.DefaultProvider;
                _signals.Trigger(DefaultBreadcrumbsService.PatternsCacheKey);
            }

            return Editor(part, shapeHelper);
        }
    }
}