using Orchard.ContentManagement;

namespace Contrib.Stars.Models {
    public class StarsPart : ContentPart {
        public bool ShowStars { get; set; }
        public bool AllowAnonymousRatings { get; set; }
        public double ResultValue { get; set; }
        public double UserRating { get; set; }
    }
}