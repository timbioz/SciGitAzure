using Onestop.Navigation.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Models;
using Orchard.Core.Navigation.Models;

namespace Onestop.Navigation.Utilities {
    public static class QueryExtensions {
        public static IContentQuery<TPart, TRecord> WithQueryHintsForMenuItem<TPart, TRecord>(this IContentQuery<TPart, TRecord> query) 
            where TPart : IContent where TRecord : ContentPartRecord {
                return query.WithQueryHints(new QueryHints().ExpandRecords<CommonPartRecord, MenuPartRecord, VersionInfoPartRecord, ExtendedMenuItemPartRecord, IdentityPartRecord, MenuItemPartRecord>());
        }
    }
}