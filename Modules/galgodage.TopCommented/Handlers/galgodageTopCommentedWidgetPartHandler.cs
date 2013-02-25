using galgodage.TopCommented.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace galgodage.TopCommented.Handlers {
    public class galgodageTopCommentedWidgetPartHandler : ContentHandler {
        public galgodageTopCommentedWidgetPartHandler(IRepository<galgodageTopCommentedWidgetPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}