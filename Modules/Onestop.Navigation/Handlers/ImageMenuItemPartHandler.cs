using Onestop.Navigation.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Onestop.Navigation.Handlers {
    public class ImageMenuItemPartHandler : ContentHandler {
        public ImageMenuItemPartHandler(IRepository<ImageMenuItemPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}