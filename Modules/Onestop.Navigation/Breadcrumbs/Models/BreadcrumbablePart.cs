using Orchard.ContentManagement;

namespace Onestop.Navigation.Breadcrumbs.Models
{
    public class BreadcrumbablePart : ContentPart<BreadcrumbablePartRecord>
    {
        /// <summary>
        /// Name of the provider for this item.
        /// </summary>
        public string Provider
        {
            get { return Record.Provider; } 
            set { Record.Provider = value; }
        }
    }
}