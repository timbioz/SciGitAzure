using Orchard.ContentManagement.Drivers;
using Contrib.Taxonomies.Models;
using Orchard.Environment.Extensions;

namespace Onestop.Navigation.Drivers {
    [OrchardFeature("Onestop.Navigation.TaxonomyOrder")]
    public class TermPartDriver : ContentPartDriver<TermPart> {

        protected override string Prefix { get { return "Term"; } }

        protected override DriverResult Editor(TermPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Taxonomies_Term_OrderLink",
                    () => shapeHelper.EditorTemplate(
                        TemplateName: "Parts/Taxonomies.Term.OrderLink",
                        Model: part,
                        Prefix: Prefix));
        }
    }
}