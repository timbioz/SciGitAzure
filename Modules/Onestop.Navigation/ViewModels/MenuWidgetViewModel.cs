using System.Collections.Generic;
using Onestop.Navigation.Models;
using Orchard.ContentManagement;

namespace Onestop.Navigation.ViewModels {
    public class MenuWidgetViewModel {
        public IEnumerable<IContent> Menus { get; set; }
        public IEnumerable<MenuWidgetMode> Modes { get; set; }
        public bool CutOrFlattenLower { get; set; }
        public int Levels { get; set; }
        public int CurrentMenuId { get; set; }
        public MenuWidgetMode Mode { get; set; }
        public string RootNode { get; set; }
        public bool WrapChildrenInDivs { get; set; }
    }
}