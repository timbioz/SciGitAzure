using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Tasks.Scheduling;

namespace Onestop.Navigation.ViewModels {
    public class CsvImportIndexViewModel {
        public IContent Menu { get; set; }
        public string Text { get; set; }
    }
}