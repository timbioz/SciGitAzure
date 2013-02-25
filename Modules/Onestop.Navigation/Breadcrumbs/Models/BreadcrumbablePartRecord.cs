using Orchard.ContentManagement.Records;

namespace Onestop.Navigation.Breadcrumbs.Models {
    public class BreadcrumbablePartRecord : ContentPartRecord {
        public virtual string Provider { get; set; }
    }
}