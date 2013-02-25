using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Contrib.Stars.Settings {
    public class StarsTypePartSettings {
        private bool? _showStars;
        public bool ShowStars {
            get {
                if (_showStars == null)
                    _showStars = true;
                return (bool)_showStars;
            }
            set { _showStars = value; }
        }

        private bool? _allowAnonymousRatings;
        public bool AllowAnonymousRatings {
            get {
                if (_allowAnonymousRatings == null)
                    _allowAnonymousRatings = false;
                return (bool)_allowAnonymousRatings;
            }
            set { _allowAnonymousRatings = value; }
        }
    }

    public class ContainerSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "StarsPart")
                yield break;

            var model = definition.Settings.GetModel<StarsTypePartSettings>();

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "StarsPart")
                yield break;

            var model = new StarsTypePartSettings();
            updateModel.TryUpdateModel(model, "StarsTypePartSettings", null, null);
            builder.WithSetting("StarsTypePartSettings.ShowStars", model.ShowStars.ToString());
            builder.WithSetting("StarsTypePartSettings.AllowAnonymousRatings", model.AllowAnonymousRatings.ToString());

            yield return DefinitionTemplate(model);
        }
    }
}