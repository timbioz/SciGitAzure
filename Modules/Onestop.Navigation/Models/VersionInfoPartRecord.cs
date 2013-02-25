using Orchard.ContentManagement.Records;

namespace Onestop.Navigation.Models {
    /// <summary>
    /// Additional version information used by version manager.
    /// </summary>
    public class VersionInfoPartRecord : ContentPartVersionRecord  {
        /// <summary>
        /// Is item removed?
        /// </summary>
        public virtual bool Removed { get; set; }

        /// <summary>
        /// Id of the author of this version.
        /// </summary>
        public virtual int Author { get; set; }

        /// <summary>
        /// Is item a draft?
        /// </summary>
        public virtual bool Draft { get; set; }
    }
}