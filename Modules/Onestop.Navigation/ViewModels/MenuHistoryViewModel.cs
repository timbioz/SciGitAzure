using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Tasks.Scheduling;

namespace Onestop.Navigation.ViewModels {
    public class MenuHistoryViewModel {
        public bool DisplayDraftRow { get; set; }
        public IEnumerable<IContent> Versions { get; set; }
        public IEnumerable<IScheduledTask> Tasks { get; set; }
        public IContent Menu { get; set; }
        public dynamic Pager { get; set; }
        public string CurrentDate { get; set; }
    }
}