//using System.Collections.Generic;
//using System.Linq;
//using Onestop.Navigation.Breadcrumbs.Services;
//using Onestop.Navigation.Breadcrumbs.ViewModels;
//using Orchard.ContentManagement;
//using Orchard.ContentManagement.MetaData;
//using Orchard.ContentManagement.MetaData.Builders;
//using Orchard.ContentManagement.MetaData.Models;
//using Orchard.ContentManagement.ViewModels;
//using Orchard.Environment.Extensions;

//namespace Onestop.Navigation.Breadcrumbs.Settings
//{
//    [OrchardFeature("Onestop.Navigation.Breadcrumbs")]
//    public class BreadcrumbsSettingsHooks : ContentDefinitionEditorEventsBase {
//        private readonly IBreadcrumbsService _service;

//        public const string Prefix = "BreadcrumbsSettings";
//        public const string TemplateName = "BreadcrumbsSettings";

//        public BreadcrumbsSettingsHooks(IBreadcrumbsService service) {
//            _service = service;
//        }

//        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
//            return Editor(definition, definition.PartDefinition.Name);
//        }

//        public override IEnumerable<TemplateViewModel> PartEditor(ContentPartDefinition definition) {
//            return Editor(definition, definition.Name);
//        }

//        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
//            return EditorUpdate(builder, builder.Name, updateModel);
//        }

//        public override IEnumerable<TemplateViewModel> PartEditorUpdate(ContentPartDefinitionBuilder builder, IUpdateModel updateModel) {
//            return EditorUpdate(builder, builder.Name, updateModel);
//        }

//        private IEnumerable<TemplateViewModel> Editor(dynamic definition, string partName)
//        {
//            if (partName != "BreadcrumbablePart") yield break;

//            BreadcrumbsSettingsViewModel model = definition.Settings.GetModel<BreadcrumbsSettingsViewModel>(Prefix);
//            model.Providers = _service.GetProviderDescriptors();

//            yield return DefinitionTemplate(model, TemplateName, Prefix);
//        }

//        private IEnumerable<TemplateViewModel> EditorUpdate(dynamic builder, string partName, IUpdateModel updateModel)
//        {
//            if (partName != "BreadcrumbablePart") yield break;

//            var model = new BreadcrumbsSettingsViewModel();
//            updateModel.TryUpdateModel(model, Prefix, null, null);

//            if (model.UseDefault) model.DefaultProvider = null;
//            model.Providers = _service.GetProviderDescriptors();

//            builder.WithSetting("BreadcrumbsSettings.DefaultProvider", !string.IsNullOrWhiteSpace(model.DefaultProvider) ? model.DefaultProvider : null);
//            yield return DefinitionTemplate(model, TemplateName, Prefix);
//        }
//    }
//}