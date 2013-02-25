using Orchard.Core.Navigation.Services;
using System.Collections.Generic;
using System.Web.Mvc;
using Onestop.Navigation.Models;
using Orchard.Security.Permissions;

namespace Onestop.Navigation.ViewModels {
    public class ItemEditorViewModel {
        public string Type { get; set; }
        public ExtendedMenuItemPart Part { get; set; }
        public string AddMediaPath { get; set; }
        public SelectList AvailableItems { get; set; }
        public IDictionary<string, IEnumerable<Permission>> Permissions { get; set; }
        public IEnumerable<MenuItemDescriptor> MenuItemDescriptors { get; set; }
        public int VersionId { get; set; }
    }
}