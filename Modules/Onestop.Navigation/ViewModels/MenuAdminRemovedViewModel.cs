using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Onestop.Navigation.ViewModels {
    public class MenuAdminRemovedViewModel {
        public IEnumerable<IContent> Items { get; set; }
        public IContent Menu { get; set; }
        public int VersionNumber { get; set; }
    }
}