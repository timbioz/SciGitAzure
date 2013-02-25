using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Onestop.Navigation.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Tasks.Scheduling;

namespace Onestop.Navigation.Services {
    public interface IMenuService : IDependency {
        /// <summary>
        /// Returns a list of menu item versions scheduled to be published.
        /// </summary>
        /// <param name="menuId">Id of a menu content item.</param>
        /// <returns></returns>
        IEnumerable<IScheduledTask> GetScheduledMenuVersions(int menuId);

        /// <summary>
        /// The delete menu item.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        /// <param name="versionId">(Optional) Version to delete.</param>
        void DeleteMenuItem(int itemId, int versionId = 0);

        /// <summary>
        /// Restores a given removed menu item version.
        /// </summary>
        /// <param name="versionId">Id of a version to restore</param>
        /// <param name="newVersion">Should create a new draft version instead of simply undeleting a given version?</param>
        void UndeleteMenuItem(int versionId, bool newVersion);

        /// <summary>
        /// Retrieves a menu by its name.
        /// </summary>
        /// <param name="menuName">The menu name.</param>
        /// <param name="options"></param>
        /// <returns>A menu content item or null if not found.</returns>
        IContent GetMenu(string menuName, VersionOptions options = null);

        /// <summary>
        /// Retrieves a menu by its id.
        /// </summary>
        /// <param name="menuId">The menu content item id.</param>
        /// <param name="options"></param>
        /// <returns>A menu content item or null if not found.</returns>
        IContent GetMenu(int menuId, VersionOptions options = null);

        /// <summary>
        /// Gets the given menu change history.
        /// </summary>
        /// <param name="menuId">The menu content item id.</param>
        /// <returns>List of menu versions</returns>
        IEnumerable<IContent> GetMenuHistory(int menuId);

        /// <summary>
        /// Lists all removed menu items.
        /// </summary>
        /// <param name="menuId">The menu content item id.</param>
        /// <param name="menuVersionNumber">(Optional) Version of the menu to get items for.</param>
        /// <returns>List of removed menu items.</returns>
        IEnumerable<IContent> GetRemovedMenuItems(int menuId, int menuVersionNumber = 0);

        /// <summary>
        /// Retrieves a single menu item.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        /// <param name="options">Optional version options.</param>
        /// <param name="baseVersionIdForDraft">Optional base version to build draft on. Used only in conjunction with VersionOptions.DraftRequired passed as options.</param>
        /// <returns>Menu item.</returns>
        IContent GetMenuItem(int itemId, VersionOptions options = null, int baseVersionIdForDraft = default(int));

        /// <summary>
        /// Retrieves all menu items for specified menu.
        /// </summary>
        /// <param name="menu">The menu to get items for.</param>
        /// <param name="options">Optional version options.</param>
        /// <param name="enforceConsistency">Should enforce consistency by unpublishing invalid items?</param>
        /// <param name="predicate">Optional predicate for item filtering on database level.</param>
        /// <param name="preloadProperties">Should menu item properties be preloaded ahead of time?</param>
        /// <returns>List of published menu items or, if 'options' are specified, list of items for a specific menu version.</returns>
        IEnumerable<IContent> GetMenuItems(IContent menu, VersionOptions options = null, Expression<Func<ExtendedMenuItemPartRecord, bool>> predicate = null, bool enforceConsistency = true, bool preloadProperties = true);

        /// <summary>
        /// Creates a menu item with a given content type.
        /// </summary>
        /// <param name="menuId">Id of a menu to bind created item to.</param>
        /// <param name="type">Content type to create.</param>
        /// <returns>New menu item.</returns>
        IContent CreateMenuItem(int menuId, string type);

        /// <summary>
        /// Publishes a given menu.
        /// </summary>
        /// <param name="menuId">Id of the menu content item.</param>
        /// <param name="versionId">Optional version identifier of the menu version to be published.</param>
        void PublishMenu(int menuId, int versionId = 0);

        /// <summary>
        /// Sets up a scheduled publishing task for a given menu.
        /// </summary>
        /// <param name="menuId">Menu content item id.</param>
        /// <param name="scheduledPublishUtc">Date and time to publish the menu.</param>
        /// <param name="baseMenuVersion">Base menu version to build the new one from.</param>
        void SchedulePublication(int menuId, DateTime scheduledPublishUtc, int? baseMenuVersion = null);

        /// <summary>
        /// Creates a new version of the menu.
        /// </summary>
        /// <param name="menuId">Identifier of the menu.</param>
        /// <param name="versionNumber">Optional version number to base new version on.</param>
        /// <returns>Number of the version created.</returns>
        IContent CreateNewVersion(int menuId, int? versionNumber = null);

        /// <summary>
        /// Deletes a given version of the menu.
        /// </summary>
        /// <param name="menuId">Ide of the menu content item.</param>
        /// <param name="versionId">Number of the version to delete.</param>
        void DeleteVersion(int menuId, int versionId);

        /// <summary>
        /// Unpublishes a given menu item, making it a draft.
        /// </summary>
        /// <param name="menuId">Menu content item id.</param>
        /// <param name="itemId">Item.</param>
        void UnpublishMenuItem(int menuId, int itemId);

        /// <summary>
        /// Computes new positions for given items.
        /// </summary>
        /// <param name="parentId">Parent item id.</param>
        /// <param name="newChildren">List of child items.</param>
        /// <param name="menuId"></param>
        void UpdatePositionsFor(int menuId, int parentId, IEnumerable<int> newChildren);

        /// <summary>
        /// Computes new positions for given items.
        /// </summary>
        /// <param name="parentVersionId">Parent item id.</param>
        /// <param name="newChildrenVersions">List of child items.</param>
        /// <param name="menuId"></param>
        void UpdatePositionsForPreview(int menuId, int parentVersionId, IEnumerable<int> newChildrenVersions);

        /// <summary>
        /// Clears all caches for a given menu.
        /// </summary>
        /// <param name="menuId">Menu content item id.</param>
        void ClearCache(int menuId);

        /// <summary>
        /// Clears draft (if any) of a given menu item.
        /// </summary>
        /// <param name="id">Menu item id.</param>
        void ClearDraft(int id);

        /// <summary>
        /// Clears all existing drafts for a given menu.
        /// </summary>
        /// <param name="menuId">Menu content item id.</param>
        void ClearAllDrafts(int menuId);

        /// <summary>
        /// Sets an arbitrary position string as a position for a given item.
        /// </summary>
        /// <param name="menuId">The menu content item id.</param>
        /// <param name="itemId">The item id.</param>
        /// <param name="newPosition">The new position.</param>
        /// <returns>
        /// </returns>
        bool SetPosition(int menuId, int itemId, string newPosition);

        /// <summary>
        /// Retrieves all menus.
        /// </summary>
        /// <returns>Collection of menus. </returns>
        IEnumerable<IContent> GetMenus();

        void SchedulePublication(IContent version, DateTime scheduledPublishUtc);
        void CancelSchedule(IContent version);

        /// <summary>
        /// Builds a query for menu items for further processing.
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="options"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        IContentQuery<ExtendedMenuItemPart> GetMenuItemsQuery(IContent menu, VersionOptions options = null, Expression<Func<ExtendedMenuItemPartRecord, bool>> predicate = null);

        /// <summary>
        /// Counts menu items that match given criteria.
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="options"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        int GetMenuItemsCount(IContent menu, VersionOptions options = null, Expression<Func<ExtendedMenuItemPartRecord, bool>> predicate = null);
    }
}