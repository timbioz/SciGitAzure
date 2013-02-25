using Contrib.Taxonomies.Services;
using Contrib.Taxonomies.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Contrib.Taxonomies.Handlers {
    public class TermPartHandler : ContentHandler {
        public TermPartHandler(IRepository<TermPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));

            OnInitializing<TermPart>((context, part) => part.Selectable = true);
        }

    }
}