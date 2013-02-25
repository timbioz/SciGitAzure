using Orchard.ContentManagement;

namespace Onestop.Navigation.Breadcrumbs.Models {
    public class BreadcrumbsSiteSettingsPart : ContentPart<BreadcrumbsSiteSettingsPartRecord>
    {
        public string DefaultProvider {
            get { return Record.DefaultProvider; }
            set { Record.DefaultProvider = value; }
        }
    }
}