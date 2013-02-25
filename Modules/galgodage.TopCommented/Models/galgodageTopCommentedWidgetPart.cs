using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace galgodage.TopCommented.Models {
    public class galgodageTopCommentedWidgetPart : ContentPart<galgodageTopCommentedWidgetPartRecord> {
        public string ForContentPart {
            get { return Record.ForContentPart; }
            set { Record.ForContentPart = value; }
        }

        //public string OrderBy {
        //    get { return Record.OrderBy; }
        //    set { Record.OrderBy = value; }
        //}

        [Required]
        public int Count {
            get { return Record.Count; }
            set { Record.Count = value; }
        }
    }
}
