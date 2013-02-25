using JetBrains.Annotations;
using Onestop.Navigation.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;

namespace Onestop.Navigation.Scheduling {
    [UsedImplicitly]
    public class PublishMenuTaskHandler : IScheduledTaskHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly IMenuService _menus;

        public PublishMenuTaskHandler(IContentManager contentManager, IOrchardServices orchardServices, IMenuService menus) {
            _orchardServices = orchardServices;
            _menus = menus;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType == PublishMenuTaskManager.PublishTaskType) {
                if (context.Task.ContentItem.Has<TitlePart>() && context.Task.ContentItem.ContentType == "Menu") {
                    Logger.Information("Publishing menu '{0}', version {1} scheduled at {2} utc",
                                       context.Task.ContentItem.As<TitlePart>().Title,
                                       context.Task.ContentItem.Version,
                                       context.Task.ScheduledUtc);

                    _menus.PublishMenu(context.Task.ContentItem.Id, context.Task.ContentItem.VersionRecord.Id);
                    _orchardServices.ContentManager.Flush();
                }
            }
        }
    }
}
