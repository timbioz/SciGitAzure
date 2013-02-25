using Orchard.ContentManagement;

namespace Onestop.Navigation.Breadcrumbs.Services
{
    /// <summary>
    /// Describes a breadcrumb segment.
    /// </summary>
    public class Segment
    {
        public Segment()
        {
            Displayed = true;
        }

        private string _displayText;

        /// <summary>
        /// Content related to this segment.
        /// </summary>
        public IContent Content { get; set; }

        /// <summary>
        /// Displayed text of this segment.
        /// </summary>
        public string DisplayText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_displayText) && Content != null) {
                    return Content.ContentItem.ContentManager.GetItemMetadata(Content).DisplayText;
                }
                return _displayText;
            }
            set { _displayText = value; }
        }

        /// <summary>
        /// Url of this segment.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Index of this segment in the Breadcrumbs path.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Should this segment be displayed?
        /// </summary>
        public bool Displayed { get; set; }
    }
}