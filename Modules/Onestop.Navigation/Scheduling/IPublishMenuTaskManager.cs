using System;
using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Tasks.Scheduling;

namespace Onestop.Navigation.Scheduling {
    public interface IPublishMenuTaskManager : IDependency {
        IEnumerable<IScheduledTask> GetMenuPublishTasks(int menuId);
        void SchedulePublication(ContentItem item, DateTime scheduledUtc);
        void DeleteTasks(ContentItem item, Func<IScheduledTask, bool> predicate = null);
    }
}