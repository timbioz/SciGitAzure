using Timbioz.RawHtmlWidget.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Timbioz.RawHtmlWidget.Drivers
{
    public class RawcodeDriver : ContentPartDriver<RawcodePart> 
    {
        protected override DriverResult Display(
            RawcodePart part, string displayType, dynamic shapeHelper)
        {

            return ContentShape("Parts_Rawcode", () => shapeHelper.Parts_Rawcode(
                Html: part.Html,
                Js: part.Js));
        }

        //GET
        protected override DriverResult Editor(
            RawcodePart part, dynamic shapeHelper)
        {

            return ContentShape("Parts_Rawcode_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Rawcode",
                    Model: part,
                    Prefix: Prefix));
        }
        //POST
        protected override DriverResult Editor(
            RawcodePart part, IUpdateModel updater, dynamic shapeHelper)
        {

            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}