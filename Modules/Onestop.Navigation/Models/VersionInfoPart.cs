using Orchard.ContentManagement;
using Orchard.Core.Common.Utilities;
using Orchard.Security;

namespace Onestop.Navigation.Models {
    public class VersionInfoPart : ContentPart<VersionInfoPartRecord>, IVersionAspect {
        private readonly LazyField<IUser> _author = new LazyField<IUser>();

        public bool Removed {
            get { return Record.Removed; }
            set { Record.Removed = value; }
        }

        public bool Draft {
            get { return Record.Draft; }
            set { Record.Draft = value; }
        }

        public LazyField<IUser> AuthorField { get { return _author; } }

        public IUser Author {
            get { return _author.Value; }
            set { _author.Value = value; }
        }

        public bool Latest {
            get { return ContentItem.VersionRecord.Latest; }
            set { ContentItem.VersionRecord.Latest = value; }
        }

        public bool Published {
            get { return ContentItem.VersionRecord.Published; }
            set { ContentItem.VersionRecord.Published = value; }
        }
    }
}