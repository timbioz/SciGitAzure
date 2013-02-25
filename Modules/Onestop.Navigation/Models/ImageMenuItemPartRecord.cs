using Orchard.ContentManagement.Records;

namespace Onestop.Navigation.Models {
    /// <summary>
    /// Record class holding information about image menu item.
    /// </summary>
    public class ImageMenuItemPartRecord : ContentPartVersionRecord {
        public virtual string Url { get; set; }
        public virtual string AlternateText { get; set; }
        public virtual string Class { get; set; }
        public virtual string Style { get; set; }
        public virtual string Alignment { get; set; }
        public virtual int? Width { get; set; }
        public virtual int? Height { get; set; } 
    }
}