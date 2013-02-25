using Orchard.ContentManagement.Records;

namespace Onestop.Navigation.Models {
    public class OnestopMenuWidgetPartRecord : ContentPartRecord {
        public virtual bool CutOrFlattenLower { get; set; }
        public virtual int Levels { get; set; }
        public virtual ContentItemRecord Menu { get; set; }
        public virtual MenuWidgetMode Mode { get; set; }
        public virtual string RootNode { get; set; }
        public virtual bool WrapChildrenInDivs { get; set; }
    }
}