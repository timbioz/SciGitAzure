using Orchard.ContentManagement.Records;
using Orchard.ContentManagement;

namespace Onestop.Navigation.Models {
    public class OnestopMenuWidgetPart : ContentPart<OnestopMenuWidgetPartRecord> {
        /// <summary>
        /// Should cut or flatten levels below threshold.
        /// </summary>
        public bool CutOrFlattenLower {
            get { return Record.CutOrFlattenLower; }
            set { Record.CutOrFlattenLower = value; }
        }

        /// <summary>
        /// How many levels to display max. (0 = all).
        /// </summary>
        public int Levels {
            get { return Record.Levels; }
            set { Record.Levels = value; }
        }

        /// <summary>
        /// Menu this widget is displaying items for.
        /// </summary>
        public ContentItemRecord Menu {
            get { return Record.Menu; }
            set { Record.Menu = value; }
        }

        /// <summary>
        /// Menu part display mode
        /// </summary>
        public MenuWidgetMode Mode {
            get { return Record.Mode; }
            set { Record.Mode = value; }
        }

        /// <summary>
        /// The root node (position - dot-notated) from which to start display
        /// </summary>
        public string RootNode {
            get { return Record.RootNode; }
            set { Record.RootNode = value; }
        }

        /// <summary>
        /// Should children be wrapped in div tags
        /// </summary>
        public bool WrapChildrenInDiv {
            get { return Record.WrapChildrenInDivs; }
            set { Record.WrapChildrenInDivs = value; }
        }
    }
}