using Onestop.Navigation.Breadcrumbs.Models;
using Onestop.Navigation.Breadcrumbs.Services;
using Onestop.Navigation.Breadcrumbs.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;

namespace Onestop.Navigation.Breadcrumbs.Drivers
{
    [OrchardFeature("Onestop.Navigation.Breadcrumbs")]
    public class BreadcrumbablePartDriver : ContentPartDriver<BreadcrumbablePart>
    {
        private readonly IBreadcrumbsService _service;
        private const string TemplateName = "Parts/Menu.Breadcrumbable.Edit";
        protected override string Prefix
        {
            get { return "Breadcrumbable"; }
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public BreadcrumbablePartDriver(IBreadcrumbsService service)
        {
            _service = service;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        protected override DriverResult Editor(BreadcrumbablePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_Menu_Breadcrumbable_Edit", () =>
            {
                var breadcrumbs = _service.Build(part);
                var model = new BreadcrumbsSettingsViewModel
                {
                    DefaultProvider = part.Provider,
                    Providers = _service.GetProviderDescriptors(),
                    UseDefault = string.IsNullOrWhiteSpace(part.Provider),
                    Preview = shapeHelper.Breadcrumbs(Breadcrumbs: breadcrumbs, ContentItem: part.ContentItem, Separator: "\\"),
                    SiteDefaultProvider = _service.GetDefaultProvider()
                };

                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix);
            });
        }

        protected override DriverResult Editor(BreadcrumbablePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            var model = new BreadcrumbsSettingsViewModel();
            if (updater.TryUpdateModel(model, Prefix, null, null))
            {
                part.Provider = model.UseDefault ? null : model.DefaultProvider;
            }
            return Editor(part, shapeHelper);
        }
    }
}