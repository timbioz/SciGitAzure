using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Onestop.Navigation.Models;
using Onestop.Navigation.Scheduling;
using Onestop.Navigation.Utilities;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Models;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Tasks.Scheduling;
using Orchard.UI;

namespace Onestop.Navigation.Services
{
    public class DefaultMenuService : Component, IMenuService
    {
        public const string MenuItemsCacheSignal = "Onestop.Navigation.MenuItems";

        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly ISignals _signals;
        private readonly IPublishMenuTaskManager _tasks;
        private readonly IVersionManager _versions;
        private readonly IOrchardServices _services;
        private readonly IRepository<ContentItemVersionRecord> _contentItemVersionRepository;

        public DefaultMenuService(
            IClock clock,
            IContentManager contentManager,
            ISignals signals,
            IPublishMenuTaskManager tasks,
            IVersionManager versions,
            IOrchardServices services,
            IRepository<ContentItemVersionRecord> contentItemVersionRepository)
        {
            _clock = clock;
            _contentManager = contentManager;
            _signals = signals;
            _tasks = tasks;
            _versions = versions;
            _services = services;
            _contentItemVersionRepository = contentItemVersionRepository;
        }

        public IEnumerable<IScheduledTask> GetScheduledMenuVersions(int menuId)
        {
            return _tasks.GetMenuPublishTasks(menuId);
        }

        public void DeleteMenu(int menuId)
        {
            var item = GetMenu(menuId);
            if (item == null || item.ContentItem == null)
            {
                return;
            }

            // Remove menu items
            foreach (var i in GetMenuItems(item, VersionOptions.Latest))
            {
                _contentManager.Remove(i.ContentItem);
            }

            // Remove the menu
            _contentManager.Remove(item.ContentItem);
            _contentManager.Flush();
        }

        /// <summary>
        /// Marks the item to delete when menu gets published.
        /// </summary>
        /// <param name="itemId">
        /// The item id.
        /// </param>
        /// <param name="versionId">Version of the item to delete.</param>
        public void DeleteMenuItem(int itemId, int versionId = 0)
        {
            var item = versionId > 0
                ? GetMenuItem(-1, VersionOptions.VersionRecord(versionId))
                : GetMenuItem(itemId, VersionOptions.DraftRequired);

            if (item == null || item.ContentItem == null)
            {
                return;
            }

            Expression<Func<ExtendedMenuItemPartRecord, bool>> predicate = i => i.Position != null && i.Position != "0" && i.Position.StartsWith(item.As<ExtendedMenuItemPart>().Position + ".");
            var children = versionId > 0
                ? GetMenuItems(item.As<MenuPart>().Menu, VersionOptions.VersionRecord(item.As<ExtendedMenuItemPart>().Menu.ContentItem.VersionRecord.Id), predicate)
                : GetMenuItems(item.As<MenuPart>().Menu, VersionOptions.Latest, predicate).ToList();

            foreach (var i in children.Concat(new[] { item }))
            {
                if (!item.As<ExtendedMenuItemPart>().IsDraft)
                {
                    var draft = versionId > 0 ? i : GetMenuItem(i.Id, VersionOptions.DraftRequired);
                    draft.As<VersionInfoPart>().Removed = true;
                }
                else
                {
                    ClearDraft(itemId);
                }
            }
        }

        public void UndeleteMenuItem(int versionId, bool newVersion)
        {
            var item = GetMenuItem(-1, VersionOptions.VersionRecord(versionId));

            if (item == null || item.ContentItem == null)
            {
                return;
            }

            if (newVersion)
            {
                // if we're trying to undelete some version, we should create a new version
                // for that we need to temporarily mark fetched version as latest
                item.ContentItem.VersionRecord.Latest = true;
                item = GetMenuItem(item.Id, VersionOptions.DraftRequired);
                item.As<ExtendedMenuItemPart>().Position = null;
                item.As<ExtendedMenuItemPart>().MenuVersion = null;
            }

            item.As<VersionInfoPart>().Removed = false;
        }

        public IContent GetMenu(string menuName, VersionOptions options = null)
        {
            var item = _contentManager.Query<TitlePart, TitlePartRecord>()
                .Where(x => x.Title == menuName)
                .ForType("Menu")
                .Slice(0, 1)
                .FirstOrDefault();

            if (item != null)
            {
                if (options == null)
                {
                    return item;
                }

                return _contentManager.Get(item.Id, options);
            }

            return null;
        }

        public IContent GetMenu(int menuId, VersionOptions options = null)
        {
            return _contentManager.Get(menuId, options ?? VersionOptions.Published);
        }

        public IEnumerable<IContent> GetMenuHistory(int menuId)
        {
            return _contentManager.GetAllVersions(menuId)
                .OrderBy(item => item.Version)
                .ToList();
        }

        public IEnumerable<IContent> GetRemovedMenuItems(int menuId, int menuVersionNumber = 0)
        {
            if (menuVersionNumber > 0)
            {
                // Get items marked as removed in that version
                var versionRecord = _contentItemVersionRepository.Get(r => r.ContentItemRecord.Id == menuId && r.Number == menuVersionNumber);

                return _contentManager
                    .Query<ExtendedMenuItemPart, ExtendedMenuItemPartRecord>(VersionOptions.AllVersions)
                    .WithQueryHintsForMenuItem()
                    .Where(i => i.MenuVersionRecord.Id == versionRecord.Id)
                    .Join<MenuPartRecord>()
                    .Where(m => m.MenuId == menuId)
                    .Join<VersionInfoPartRecord>()
                    .Where(v => v.Removed)
                    .List();
            }

            // Get items not existing at all in a given menu (neither latest nor published)
            var versions = _contentManager
                .Query<ExtendedMenuItemPart, ExtendedMenuItemPartRecord>(VersionOptions.AllVersions)
                .WithQueryHintsForMenuItem()
                .Join<MenuPartRecord>()
                .Where(m => m.MenuId == menuId)
                .Join<VersionInfoPartRecord>()
                .Where(v => v.Removed)
                .List()
                .Where(i => !i.ContentItem.Record.Versions.Any(v => v.Published || v.Latest))
                .GroupBy(item => item.Id)
                .Select(g => g.OrderBy(item => item.ContentItem.Version).Last());

            return versions;
        }

        /// <summary>
        /// The get menu item.
        /// </summary>
        /// <param name="itemId">
        /// The item id.
        /// </param>
        /// <param name="options">Optional version options.</param>
        /// <param name="baseVersionIdForDraft">Optional base version to build draft on. Used only in conjunction with VersionOptions.DraftRequired passed as options.</param>
        /// <returns>
        /// Menu item.
        /// </returns>
        public IContent GetMenuItem(int itemId, VersionOptions options = null, int baseVersionIdForDraft = default(int))
        {
            if (options != null && options.IsDraftRequired)
            {
                // Build draft if necessary, based on latest/specific version
                var item = baseVersionIdForDraft != default(int)
                    ? _contentManager.Get(itemId, VersionOptions.VersionRecord(baseVersionIdForDraft))
                    : _contentManager.Get(itemId, VersionOptions.Latest);

                if (item != null)
                {
                    item = _versions.GetDraft(item);
                    item.As<ExtendedMenuItemPart>().MenuVersion = null;
                    return item;
                }

                return null;
            }

            return _contentManager.Get(itemId, options ?? VersionOptions.Latest).As<ExtendedMenuItemPart>();
        }

        public IContent CreateMenuItem(int menuId, string type)
        {
            var item = _contentManager.New(type);

            if (item.Is<ExtendedMenuItemPart>())
            {
                var menu = GetMenu(menuId);

                item.As<ExtendedMenuItemPart>().Position = null;
                item.As<ExtendedMenuItemPart>().DisplayHref = true;
                item.As<ExtendedMenuItemPart>().DisplayText = true;
                item.As<ExtendedMenuItemPart>().MenuVersion = null;
                item.As<MenuPart>().Menu = menu;
            }
            return item;
        }

        /// <summary>
        /// Gets the existing menus.
        /// </summary>
        /// <returns>
        /// Collection of menus.
        /// </returns>
        public IEnumerable<IContent> GetMenus()
        {
            try
            {
                return _contentManager.Query<TitlePart, TitlePartRecord>()
                    .Where(x => x.Title != null)
                    .ForType("Menu")
                    .List();
            }
            catch (Exception)
            {

                return Enumerable.Empty<MenuPart>();
            }

        }

        public IEnumerable<IContent> GetMenuItems(IContent menu,
                                                  VersionOptions options = null,
                                                  Expression<Func<ExtendedMenuItemPartRecord, bool>> predicate = null,
                                                  bool enforceConsistency = true,
                                                  bool preloadProperties = true)
        {
            options = options ?? VersionOptions.Published;

            // Building the final query
            var results = GetMenuItemsQuery(menu, options, predicate)
                .List()
                .ToList();

            // Preloading item properties
            if (preloadProperties) {
                PreloadItemsProperties(results);
            }

            // Keeping published menus in consistent state in case something went wrong during publishing.
            // Note: This should happen in the handler, but that ends up with a SOE.
            if (enforceConsistency)
            {
                bool inconsitencyFound = false;
                foreach (var version in results.Where(v => !CheckConsistency(v)))
                {
                    inconsitencyFound = true;
                    version.Position = null;
                    version.MenuVersion = null;
                    _versions.RemoveVersion(version.ContentItem);
                    Logger.Warning("Found and removed a corrupted menu item '{0}'.", version.Id);
                }

                if (inconsitencyFound)
                {
                    _contentManager.Flush();
                    _contentManager.Clear();
                    
                    // If we encountered inconsistency, start from the beginning
                    return GetMenuItems(menu, options, predicate, false, preloadProperties);
                }
            }

            // Version number/id based options return all versions, so those must be grouped together
            return options.IsLatest || options.IsPublished
                ? results
                : results.GroupBy(i => i.ContentItem.Id, (i, group) => @group.OrderByDescending(item => item.ContentItem.VersionRecord.Id).FirstOrDefault());
        }

        public int GetMenuItemsCount(IContent menu, VersionOptions options = null, Expression<Func<ExtendedMenuItemPartRecord, bool>> predicate = null)
        {
            return GetMenuItemsQuery(menu, options, predicate).Count();
        }

        public IContentQuery<ExtendedMenuItemPart> GetMenuItemsQuery(IContent menu, VersionOptions options = null, Expression<Func<ExtendedMenuItemPartRecord, bool>> predicate = null)
        {
            options = options ?? VersionOptions.Published;
            int versionRecordId = default(int);

            // Initial query
            var query = _contentManager
                .Query<ExtendedMenuItemPart, ExtendedMenuItemPartRecord>(VersionOptions.AllVersions)
                .Where(i => i.Position != null && i.Position != "0");

            if (options.IsPublished)
            {
                //versionRecordId = _contentItemVersionRepository.Get(r => r.ContentItemRecord.Id == menu.Id && r.Published).Id;
                query = _contentManager
                    .Query<ExtendedMenuItemPart, ExtendedMenuItemPartRecord>(VersionOptions.Published)
                    .Where(i => i.Position != null && i.Position != "0"); ;
            }
            else if (options.IsLatest)
            {
                query = _contentManager.Query<ExtendedMenuItemPart, ExtendedMenuItemPartRecord>(VersionOptions.Latest);
            }
            else if (options.VersionNumber != default(int) || options.VersionRecordId != default(int))
            {
                // Seek for the appropriate version id and include in query
                if (options.VersionNumber != default(int))
                    versionRecordId = _contentItemVersionRepository.Get(r => r.ContentItemRecord.Id == menu.Id && r.Number == options.VersionNumber).Id;
                else if (options.VersionRecordId != default(int))
                    versionRecordId = options.VersionRecordId;

                // Proceed if versionId has been found based on the provided arguments
                if (versionRecordId != default(int))
                {
                    query = query.Where(i => i.MenuVersionRecord.Id == versionRecordId);
                }
                else
                {
                    throw new ArgumentException("Provided version record cannot be found", "options");
                }
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            // Building the final query
            return query
                .WithQueryHintsForMenuItem()
                .Join<VersionInfoPartRecord>()
                .Join<CommonPartRecord>()
                .Join<MenuPartRecord>()
                .Where(m => m.MenuId == menu.Id);
        }

        private void PreloadItemsProperties(IEnumerable<ExtendedMenuItemPart> items)
        {
            var itemIds = items.Select(i => i.Id)
                .Distinct()
                .ToArray();

            // Preloading item properties ahead of time.
            var changedVersions = _versions.FilterDrafts(itemIds).ToDictionary(id => id);
            var publishedVersions = _versions.FilterPublished(itemIds).ToDictionary(id => id);
            var latestVersions = _versions.FilterLatest(itemIds).ToDictionary(id => id);
            var allPublished = _contentManager
                .Query<ExtendedMenuItemPart>(VersionOptions.Published)
                .Where<ExtendedMenuItemPartRecord>(r => itemIds.Contains(r.ContentItemRecord.Id))
                .List()
                .ToDictionary(item => item.Id);

            foreach (var item in items)
            {
                item.HasPublishedField.Value = publishedVersions.ContainsKey(item.Id);
                item.HasLatestField.Value = latestVersions.ContainsKey(item.Id);
                item.IsChangedField.Value = changedVersions.ContainsKey(item.Id);
                item.PublishedVersionField.Value = !item.ContentItem.VersionRecord.Published && allPublished.ContainsKey(item.Id)
                                                       ? allPublished[item.Id]
                                                       : item;
            }
        }

        private bool CheckConsistency(ExtendedMenuItemPart item)
        {
            // If the position is there, the item has to be either current, removed or a draft
            if (item.HasPosition && !item.IsCurrent && !item.IsRemoved && !item.IsDraft)
                return false;

            // Item cannot be published without having a latest version
            if (!item.HasLatest && item.IsPublished)
                return false;

            return true;
        }

        public void PublishMenu(int menuId, int versionId = 0)
        {
            try
            {
                var menu = versionId > 0 ? GetMenu(menuId, VersionOptions.VersionRecord(versionId))
                                         : CreateNewVersion(menuId);

                // If specific version is requested then retrieve only those items.
                // If no version specified, then get all latest items
                // Filter out those with no position set
                Expression<Func<ExtendedMenuItemPartRecord, bool>> predicate = i => i.Position != null && i.Position != "0";

                // We neither have to check for consistency nor preload item properties here
                var items = GetMenuItems(menu, versionId > 0 ? VersionOptions.VersionRecord(versionId) : VersionOptions.Latest, predicate, false, false);

                _versions.PublishVersion(menu.ContentItem);
                _contentManager.Flush();

                foreach (var i in items)
                {
                    if (i.As<ExtendedMenuItemPart>().IsRemoved)
                    {
                        _versions.RemoveVersion(i.ContentItem);
                    }
                    else
                    {
                        _versions.PublishVersion(i.ContentItem);
                    }
                }

                TriggerSignal(CacheUtility.GetCacheSignal(menuId));
                _tasks.DeleteTasks(menu.ContentItem, task => task.ContentItem.Version == menu.ContentItem.Version);
                _contentManager.Flush();
            }
            catch
            {
                _services.TransactionManager.Cancel();
                throw;
            }
        }

        public void SchedulePublication(int menuId, DateTime scheduledPublishUtc, int? baseMenuVersion = null)
        {
            var version = CreateNewVersion(menuId, baseMenuVersion);
            _tasks.SchedulePublication(version.ContentItem, scheduledPublishUtc);
        }

        public void SchedulePublication(IContent version, DateTime scheduledPublishUtc)
        {
            _tasks.SchedulePublication(version.ContentItem, scheduledPublishUtc);
        }

        public void CancelSchedule(IContent version)
        {
            _tasks.DeleteTasks(version.ContentItem, task => version.ContentItem.Version == task.ContentItem.Version);
            var allMenuVersions = GetMenuHistory(version.Id);
            var scheduledVersions = GetScheduledMenuVersions(version.Id);

            // If it's scheduled then it's ok
            // If it's not scheduled and not published yet (ie. "cancelled"), then it's not ok
            var versionToSetAsLatest = allMenuVersions
                .OrderByDescending(item => item.ContentItem.Version)
                .FirstOrDefault(item =>
                    item.ContentItem.VersionRecord.Id != version.ContentItem.VersionRecord.Id &&
                    (scheduledVersions.Any(task => task.ContentItem.VersionRecord.Id == item.ContentItem.VersionRecord.Id) || item.As<CommonPart>().VersionPublishedUtc.HasValue));

            if (version.ContentItem.VersionRecord.Latest)
            {
                version.ContentItem.VersionRecord.Latest = false;
                var versionItems = GetMenuItems(version, VersionOptions.VersionRecord(version.ContentItem.Version));
                foreach (var item in versionItems)
                {
                    item.ContentItem.VersionRecord.Latest = false;
                }

                if (versionToSetAsLatest != null)
                {
                    _versions.SetLatest(versionToSetAsLatest.ContentItem);
                    var versionToSetAsLatestItems = GetMenuItems(version, VersionOptions.VersionRecord(versionToSetAsLatest.ContentItem.Version));
                    foreach (var item in versionToSetAsLatestItems)
                    {
                        // If there is a draft (item is changed) or item has been removed - do not set the latest flag
                        if (!item.As<ExtendedMenuItemPart>().IsChanged)
                        {
                            if (item.As<ExtendedMenuItemPart>().IsRemoved)
                            {
                                item.ContentItem.VersionRecord.Latest = false;
                                item.ContentItem.VersionRecord.Published = false;
                            }
                            else
                            {
                                item.ContentItem.VersionRecord.Latest = true;
                            }
                        }
                    }
                }
            }
        }

        public IContent CreateNewVersion(int menuId, int? versionNumber = null)
        {
            var menu = GetMenu(menuId);

            if (menu == null)
            {
                throw new ArgumentException("Specified menu does not exist", "menuName");
            }

            try
            {
                // Creating new, unpublished, not-latest version
                var newVersion = _versions.NewVersion(menu.ContentItem);

                // Get the list of latest (draft or published) versions of menu items
                // TODO: This should be done more performant by using a predicate parameter for database-level filtering
                // A simple predicate will help to limit the number of items fetched
                Expression<Func<ExtendedMenuItemPartRecord, bool>> predicate = i => i.Position != null && i.Position != "0" && i.ContentItemRecord.Versions.Any(v => v.Published);
                var items = versionNumber == null
                                ? GetMenuItems(menu, VersionOptions.Latest, predicate)
                                : GetMenuItems(menu, VersionOptions.Number(versionNumber.Value), predicate);

                foreach (var item in items.Where(i => i.As<ExtendedMenuItemPart>().IsCurrent).OrderBy(i => i.As<ExtendedMenuItemPart>().Position, new FlatPositionComparer()))
                {
                    var draft = _versions.GetDraft(item.ContentItem);
                    SetUpItemForVersion(draft.As<ExtendedMenuItemPart>(), newVersion);
                }

                _services.ContentManager.Flush();
                return newVersion;
            }
            catch
            {
                _services.TransactionManager.Cancel();
                throw;
            }
        }

        public void DeleteVersion(int menuId, int versionId)
        {
            throw new NotImplementedException();
        }

        private void SetUpItemForVersion(ExtendedMenuItemPart item, IContent menuVersion)
        {
            item.MenuVersion = menuVersion;
            item.As<VersionInfoPart>().Draft = false;
        }

        public void UnpublishMenuItem(int menuId, int itemId)
        {
            var menu = GetMenu(menuId);
            var parent = GetMenuItem(itemId).As<ExtendedMenuItemPart>();

            // Get child items
            Expression<Func<ExtendedMenuItemPartRecord, bool>> predicate = i => i.Position != null && i.Position != "0" && i.Position.StartsWith(parent.As<ExtendedMenuItemPart>().Position + ".");
            var childItems = GetMenuItems(menu, VersionOptions.Published, predicate, false);

            foreach (var item in childItems.Concat(new[] { parent }))
            {
                _contentManager.Unpublish(item.ContentItem);
                item.As<ExtendedMenuItemPart>().Position = null;
                item.As<ExtendedMenuItemPart>().MenuVersion = null;
            }
        }

        public void UpdatePositionsFor(int menuId, int parentId, IEnumerable<int> newChildren)
        {
            var menu = GetMenu(menuId);
            var parent = GetMenuItem(parentId, VersionOptions.Latest);
            var parentPosition = parentId > 0 && parent != null
                                     ? parent.As<ExtendedMenuItemPart>().Position + "."
                                     : string.Empty;

            // Changing new hierarchy

            int i = 1;
            var newItems =
                newChildren.Select((child, index) =>
                    {
                        var latestVersion = GetMenuItem(child, VersionOptions.Latest);
                        // Check if the position has really changed!
                        var oldPosition = latestVersion.As<ExtendedMenuItemPart>().Position ?? "";
                        var newPosition = parentPosition + (index + 1);

                        return !oldPosition.Equals(newPosition, StringComparison.OrdinalIgnoreCase)
                            ? GetMenuItem(child, VersionOptions.DraftRequired)
                            : latestVersion;
                    })
                    .Where(item => item != null && !item.As<ExtendedMenuItemPart>().IsRemoved)
                    .ToList();

            // Simple predicate to initially limit the number of items
            Expression<Func<ExtendedMenuItemPartRecord, bool>> predicate = child => child.Position != null && child.Position != "0";
            var items = GetMenuItems(menu, VersionOptions.Latest, predicate);
            var newItemsChildren = newItems.ToDictionary(
                k => k.Id,
                v => items
                    .Where(child => child.As<ExtendedMenuItemPart>().Position.StartsWith((v.As<ExtendedMenuItemPart>().Position ?? "") + "."))
                    .Select(child => GetMenuItem(child.Id, VersionOptions.DraftRequired))
                    .Where(mItem => mItem != null && !mItem.As<ExtendedMenuItemPart>().IsRemoved)
                    .ToList());

            foreach (var item in newItems)
            {
                var oldPosition = item.As<ExtendedMenuItemPart>().Position ?? "";
                var newPosition = parentPosition + i;

                if (oldPosition.Equals(newPosition, StringComparison.OrdinalIgnoreCase)) { i++; continue; }

                foreach (var child in newItemsChildren[item.Id])
                {
                    child.As<ExtendedMenuItemPart>().Position = newPosition + child.As<ExtendedMenuItemPart>().Position.Remove(0, oldPosition.Length);
                }

                item.As<ExtendedMenuItemPart>().Position = newPosition;

                if (item.Is<VersionInfoPart>())
                {
                    item.As<VersionInfoPart>().Author = _services.WorkContext.CurrentUser;
                }

                if (item.Is<CommonPart>())
                {
                    item.As<CommonPart>().VersionModifiedUtc = _clock.UtcNow;
                }
                i++;
            }

            _contentManager.Flush();
        }

        public void UpdatePositionsForPreview(int menuId, int parentVersionId, IEnumerable<int> newChildrenVersions)
        {
            var menu = GetMenu(menuId);

            var parent = GetMenuItem(-1, VersionOptions.VersionRecord(parentVersionId)).As<ExtendedMenuItemPart>();
            var parentPosition = parentVersionId > 0 && parent != null
                                     ? parent.Position + "."
                                     : string.Empty;

            // Changing new hierarchy
            int i = 1;
            var newItems = newChildrenVersions
                .Select(child => GetMenuItem(-1, VersionOptions.VersionRecord(child)))
                .Where(mItem => mItem != null && !mItem.As<ExtendedMenuItemPart>().IsRemoved)
                .ToList();

            Expression<Func<ExtendedMenuItemPartRecord, bool>> predicate = child => child.Position != null && child.Position != "0";
            var items = GetMenuItems(menu, predicate: predicate).ToList();
            var newItemsChildren = newItems.ToDictionary(
                k => k.Id,
                v => items
                    .Where(child => child.As<ExtendedMenuItemPart>().Position.StartsWith((v.As<ExtendedMenuItemPart>().Position ?? "") + "."))
                    .Select(child => GetMenuItem(-1, VersionOptions.VersionRecord(child.ContentItem.VersionRecord.Id)))
                    .Where(mItem => mItem != null && !mItem.As<ExtendedMenuItemPart>().IsRemoved)
                    .ToList());

            foreach (var item in newItems)
            {
                var oldPosition = item.As<ExtendedMenuItemPart>().Position ?? "";
                var newPosition = parentPosition + i;

                foreach (var child in newItemsChildren[item.Id])
                {
                    child.As<ExtendedMenuItemPart>().Position = newPosition + child.As<ExtendedMenuItemPart>().Position.Remove(0, oldPosition.Length);
                }

                item.As<ExtendedMenuItemPart>().Position = newPosition;
                i++;
            }
        }

        public void ClearDraft(int id)
        {
            var item = _contentManager.Get(id, VersionOptions.Latest)
                    ?? _contentManager.Get(id, VersionOptions.Published);

            if (item != null && item.Is<ExtendedMenuItemPart>())
            {
                try
                {
                    _versions.ClearDraft(item);
                }
                catch
                {
                    // Do nothing - if it fails it means that there is no draft (record nonexistent)
                }
            }
        }

        public void ClearAllDrafts(int menuId)
        {
            var menu = GetMenu(menuId);
            var items = GetMenuItems(menu, VersionOptions.AllVersions)
                .Where(i => i.As<ExtendedMenuItemPart>().HasDraft());

            foreach (var item in items)
            {
                ClearDraft(item.Id);
            }
        }

        public bool SetPosition(int menuId, int itemId, string newPosition)
        {
            var item = GetMenuItem(itemId);
            if (!item.As<ExtendedMenuItemPart>().IsDraft || item.As<ExtendedMenuItemPart>().Menu.Id != menuId)
            {
                return false;
            }

            if (!newPosition.Trim()
               .Split('.')
               .All(s => { int i; return int.TryParse(s.Trim(), out i) && i > 0; }))
            {
                return false;
            }

            try
            {
                var menu = GetMenu(menuId);
                var lastDot = newPosition.LastIndexOf('.');
                var parentPosition = lastDot != -1 ? newPosition.Substring(0, lastDot) : "";
                var parentItem = string.IsNullOrWhiteSpace(parentPosition)
                                     ? null
                                     : GetMenuItems(menu, VersionOptions.Latest, rec => rec.Position != null && rec.Position.Equals(parentPosition)).First();
                var parentLevel = newPosition.Count(c => c == '.');

                item.As<ExtendedMenuItemPart>().Position = newPosition;

                // Get the conflicting item(s)
                var siblings = GetMenuItems(menu, VersionOptions.Latest, rec => rec.Position != null && rec.Position != ""
                                                                                && rec.Position.StartsWith(parentPosition));
                var siblingIds = new[] { item.As<ExtendedMenuItemPart>() }
                    .Concat(siblings
                        .Select(s => s.As<ExtendedMenuItemPart>())
                        .Where(i => !i.IsRemoved && i.IsCurrent)
                        .Where(i => i.Position.Count(c => c == '.') == parentLevel))
                    .OrderBy(i => i.Position, new FlatPositionComparer())
                    .Select(i => i.Id);

                UpdatePositionsFor(menuId, parentItem != null ? parentItem.Id : default(int), siblingIds);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, T("Cannot set menu item {0} position to {1}.", item.Id, newPosition).Text);
                return false;
            }
        }

        public void ClearCache(int menuId)
        {
            TriggerSignal(CacheUtility.GetCacheSignal(menuId));
        }

        private void TriggerSignal(string signalName)
        {
            _signals.Trigger(signalName);
        }
    }
}