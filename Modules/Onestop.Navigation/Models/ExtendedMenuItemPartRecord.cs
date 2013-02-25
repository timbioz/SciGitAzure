using Orchard.ContentManagement.Records;

namespace Onestop.Navigation.Models {
    public class ExtendedMenuItemPartRecord : ContentPartVersionRecord {
        public virtual string Text { get; set; }
        public virtual string Url { get; set; }

        public virtual string Classes { get; set; }
        public virtual string CssId { get; set; }
        public virtual bool DisplayHref { get; set; }
        public virtual bool DisplayText { get; set; }
        public virtual string Position { get; set; }
        public virtual string ParentPosition { get; set; }
        public virtual string DraftPosition { get; set; }
        public virtual string Permission { get; set; }
        public virtual string TechnicalName { get; set; }
        public virtual string GroupName { get; set; }
        public virtual ContentItemVersionRecord MenuVersionRecord { get; set; }
        public virtual bool InNewWindow { get; set; }
        public virtual string SubTitle { get; set; }
    }
}