using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Tasks.Scheduling;

namespace Onestop.Navigation.Scheduling {
    [UsedImplicitly]
    public class PublishMenuTaskManager : IPublishMenuTaskManager {
        public const string PublishTaskType = "PublishMenu";

        private readonly IScheduledTaskManager _scheduledTaskManager;

        public PublishMenuTaskManager(IScheduledTaskManager scheduledTaskManager) {
            _scheduledTaskManager = scheduledTaskManager;
        }

        public IEnumerable<IScheduledTask> GetMenuPublishTasks(int menuId) {
            return _scheduledTaskManager
                .GetTasks(PublishTaskType)
                .Where(t => t.ContentItem.Id == menuId && t.ContentItem.ContentType == "Menu");
        }

        public void SchedulePublication(ContentItem item, DateTime scheduledUtc) {
            DeleteTasks(item, task => task.ContentItem.VersionRecord.Id == item.VersionRecord.Id);
            _scheduledTaskManager.CreateTask(PublishTaskType, scheduledUtc, item);
        }

        public void DeleteTasks(ContentItem item, Func<IScheduledTask, bool> predicate = null) {
            if (predicate != null) {
                _scheduledTaskManager.DeleteTasks(item, task => task.TaskType == PublishTaskType && predicate(task));
            }
            else {
                _scheduledTaskManager.DeleteTasks(item, task => task.TaskType == PublishTaskType);
            }
        }
    }
}