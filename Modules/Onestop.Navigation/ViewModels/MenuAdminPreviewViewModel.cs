using System;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Services;
using Orchard.Tasks.Scheduling;
using System.Collections.Generic;

namespace Onestop.Navigation.ViewModels {
    public class MenuAdminPreviewViewModel {
        public IEnumerable<IContent> Items { get; set; }
        public IEnumerable<IContent> RemovedItems { get; set; } 
        public IEnumerable<IScheduledTask> MenuVersions { get; set; }
        public IEnumerable<MenuItemDescriptor> MenuItemDescriptors { get; set; }
        public IContent Menu { get; set; }
        public int VersionNumber { get; set; }
        public string ScheduledDate { get; set; }
        public string ScheduledTime { get; set; }
        public DateTime ScheduledUtc { get; set; }
    }
}