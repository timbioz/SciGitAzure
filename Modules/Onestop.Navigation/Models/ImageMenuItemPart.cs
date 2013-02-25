using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Onestop.Navigation.Models {
    /// <summary>
    /// Content part holding data for image menu item.
    /// </summary>
    public class ImageMenuItemPart : ContentPart<ImageMenuItemPartRecord> {
        [Required]
        public string Source {
            get { return Record.Url; }
            set { Record.Url = value; }
        }
        public string AlternateText {
            get { return Record.AlternateText; }
            set { Record.AlternateText = value; }
        }
        public string Class {
            get { return Record.Class; }
            set { Record.Class = value; }
        }
        public string Style {
            get { return Record.Style; }
            set { Record.Style = value; }
        }
        public string Alignment {
            get { return Record.Alignment; }
            set { Record.Alignment = value; }
        }
        public int? Width {
            get { return Record.Width; }
            set { Record.Width = value; }
        }
        public int? Height {
            get { return Record.Height; }
            set { Record.Height = value; }
        }
    }
}