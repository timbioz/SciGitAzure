using System.ComponentModel.DataAnnotations;

namespace Onestop.Navigation.Models {
    public class AdminMenuItemRecord {
        public virtual int Id { get; set; }
        [Required]
        public virtual string Text { get; set; }
        [Required]
        public virtual string Url { get; set; }
        [Required]
        public virtual string Position { get; set; }
        public virtual string ItemGroup { get; set; }
        public virtual string GroupPosition { get; set; }
    }
}