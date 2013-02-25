using System.Linq;
using Onestop.Navigation.Models;
using Orchard;
using Orchard.ContentManagement;

namespace Onestop.Navigation.Services {
    public interface IVersionManager : IDependency {
        /// <summary>
        /// Creates new version based on provided content item.
        /// </summary>
        /// <param name="basedOn">Item version to base new version on.</param>
        /// <returns>Created version.</returns>
        ContentItem NewVersion(ContentItem basedOn);
        
        /// <summary>
        /// Gets a draft version of the provided item.
        /// </summary>
        /// <param name="item">Item to build draft for.</param>
        /// <returns>Draft version.</returns>
        ContentItem GetDraft(ContentItem item);

        /// <summary>
        /// Clears any existing draft of a provided item, setting it back to it's published state.
        /// </summary>
        /// <param name="item">Item to clear draft for.</param>
        void ClearDraft(ContentItem item);

        /// <summary>
        /// Gets the current, published version of a given item.
        /// </summary>
        /// <param name="item">Item to get the published version for.</param>
        /// <returns>Published version of an item.</returns>
        IContent GetCurrent(IContent item);

        /// <summary>
        /// Gets the version with
        /// </summary>
        /// <param name="id"></param>
        /// <param name="versionNumber"></param>
        /// <returns></returns>
        ContentItem GetVersion(int id, int versionNumber);

        /// <summary>
        /// Checks if an item has a removed version.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool HasRemovedVersion(int id);

        /// <summary>
        /// Checks if an item has a draft version.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool HasDraftVersion(int id);

        /// <summary>
        /// Checks if an item has a published version.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool HasPublishedVersion(int id);
        
        /// <summary>
        /// Publishes a given version of an item.
        /// </summary>
        /// <param name="item">Item to publish</param>
        void PublishVersion(ContentItem item);

        /// <summary>
        /// Removes a given version of the item.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        void RemoveVersion(ContentItem item);

        /// <summary>
        /// Sets given item version as latest.
        /// </summary>
        /// <param name="item">Item to set latest on.</param>
        void SetLatest(ContentItem item);

        /// <summary>
        /// Returns the underlying queryable object for quering item versions.
        /// </summary>
        /// <returns></returns>
        IQueryable<VersionInfoPartRecord> GetQueryable();

        bool HasLatestVersion(int id);
    }
}