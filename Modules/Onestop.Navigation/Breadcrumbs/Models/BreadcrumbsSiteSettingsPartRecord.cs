using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;

namespace Onestop.Navigation.Breadcrumbs.Models {
    [OrchardFeature("Onestop.Navigation.Breadcrumbs")]
    public class BreadcrumbsSiteSettingsPartRecord : ContentPartRecord {
        public virtual string DefaultProvider { get; set; }
    }
}