using System;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Services;
using Orchard.Tasks.Scheduling;
using System.Collections.Generic;
using Onestop.Navigation.Models;

namespace Onestop.Navigation.ViewModels {
    public class MenuAdminIndexViewModel {
        public IEnumerable<IScheduledTask> MenuVersions { get; set; } 
        public IEnumerable<IContent> Items { get; set; }
        public IEnumerable<MenuItemDescriptor> MenuItemDescriptors { get; set; }
        public IList<IContent> DraftItems { get; set; }
        public IList<IContent> CurrentItems { get; set; }
        public IContent Menu { get; set; }
        public string ScheduledDate { get; set; }
        public string ScheduledTime { get; set; }
        public DateTime ScheduledUtc { get; set; }
        public DisplayMode Mode { get; set; }
    }
}