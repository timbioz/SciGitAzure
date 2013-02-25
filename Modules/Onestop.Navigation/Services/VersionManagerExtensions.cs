using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;

namespace Onestop.Navigation.Services
{
    public static class VersionManagerExtensions
    {
        /// <summary>
        /// Filters a given list of item ids returning ids of items having a draft version.
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="itemIds"></param>
        public static IEnumerable<int> FilterDrafts(this IVersionManager manager, params int[] itemIds)
        {
            return manager.GetQueryable()
                          .Where(r => itemIds.Contains(r.ContentItemRecord.Id) && r.Draft)
                          .Select(v => v.ContentItemRecord.Id)
                          .Distinct();
        }

        /// <summary>
        /// Filters a given list of item ids returning ids of items having a published version.
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="itemIds"></param>
        public static IEnumerable<int> FilterPublished(this IVersionManager manager, IEnumerable<int> itemIds)
        {
            return manager.GetQueryable()
                          .Where(r => itemIds.Contains(r.ContentItemRecord.Id) && r.ContentItemVersionRecord.Published)
                          .Select(v => v.ContentItemRecord.Id)
                          .Distinct();
        }

        /// <summary>
        /// Filters a given list of item ids returning ids of items having a latest version.
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="itemIds"></param>
        public static IEnumerable<int> FilterLatest(this IVersionManager manager, IEnumerable<int> itemIds)
        {
            return manager.GetQueryable()
                          .Where(r => itemIds.Contains(r.ContentItemRecord.Id) && r.ContentItemVersionRecord.Latest)
                          .Select(v => v.ContentItemRecord.Id)
                          .Distinct();
        }
    }
}