using Onestop.Navigation.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Onestop.Navigation.Drivers {
    public class ImageMenuItemPartDriver : ContentPartDriver<ImageMenuItemPart> {
        private const string TemplateName = "Parts/Menu.ImageMenuItem.Edit";

        protected override string Prefix {
            get { return "ImageMenuItem"; }
        }

        protected override DriverResult Editor(ImageMenuItemPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Menu_ImageMenuItem_Edit", () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(ImageMenuItemPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (updater != null) {
                updater.TryUpdateModel(part, Prefix, null, null);
            }

            return Editor(part, shapeHelper);
        }
    }
}