using Orchard.ContentManagement.Records;

namespace galgodage.TopCommented.Models {
    public class galgodageTopCommentedWidgetPartRecord : ContentPartRecord {
        public virtual string ForContentPart { get; set; }
        public virtual int Count { get; set; }
        //public virtual string OrderBy { get; set; }
    }
}
