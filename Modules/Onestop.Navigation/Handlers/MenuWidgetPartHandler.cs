using Onestop.Navigation.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Onestop.Navigation.Handlers {
    public class MenuWidgetPartHandler : ContentHandler {
        public MenuWidgetPartHandler(IRepository<OnestopMenuWidgetPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}