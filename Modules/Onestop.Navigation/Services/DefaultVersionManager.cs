using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Onestop.Navigation.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Logging;

namespace Onestop.Navigation.Services {
    [UsedImplicitly]
    public class DefaultVersionManager : IVersionManager {
        private readonly IContentManager _content;
        private readonly IRepository<ContentItemVersionRecord> _contentItemVersionRepository;
        private readonly IRepository<VersionInfoPartRecord> _infoPartRepository;
        private readonly Lazy<IEnumerable<IContentHandler>> _handlers;
        private readonly IDictionary<int, ContentItem> _cache;

        public DefaultVersionManager(
            IContentManager content,
            IRepository<ContentItemVersionRecord> contentItemVersionRepository,
            Lazy<IEnumerable<IContentHandler>> handlers,
            IRepository<VersionInfoPartRecord> infoPartRepository) {
            _content = content;
            _contentItemVersionRepository = contentItemVersionRepository;
            _handlers = handlers;
            _infoPartRepository = infoPartRepository;
            Logger = NullLogger.Instance;
            _cache = new Dictionary<int, ContentItem>();
        }

        public ILogger Logger { get; set; }

        public IEnumerable<IContentHandler> Handlers {
            get { return _handlers.Value; }
        }

        public ContentItem NewVersion(ContentItem existingContentItem) {
            var contentItemRecord = existingContentItem.Record;

            // locate the existing and the current latest versions, allocate building version
            var existingItemVersionRecord = existingContentItem.VersionRecord;
            var buildingItemVersionRecord = new ContentItemVersionRecord
            {
                ContentItemRecord = contentItemRecord,
                Latest = true,
                Published = false,
                Data = existingItemVersionRecord.Data,

                // Always set up version number to max from existing to avoid conflicts
                Number = contentItemRecord.Versions.Max(x => x.Number) + 1,
            };

            var latestVersion = contentItemRecord.Versions.SingleOrDefault(x => x.Latest);

            if (latestVersion != null) {
                latestVersion.Latest = false;
            }

            contentItemRecord.Versions.Add(buildingItemVersionRecord);
            _contentItemVersionRepository.Create(buildingItemVersionRecord);

            var buildingContentItem = _content.New(existingContentItem.ContentType);
            buildingContentItem.VersionRecord = buildingItemVersionRecord;

            var context = new VersionContentContext
            {
                Id = existingContentItem.Id,
                ContentType = existingContentItem.ContentType,
                ContentItemRecord = contentItemRecord,
                ExistingContentItem = existingContentItem,
                BuildingContentItem = buildingContentItem,
                ExistingItemVersionRecord = existingItemVersionRecord,
                BuildingItemVersionRecord = buildingItemVersionRecord,
            };
            Handlers.Invoke(handler => handler.Versioning(context), Logger);
            Handlers.Invoke(handler => handler.Versioned(context), Logger);

            return context.BuildingContentItem;
        }

        public ContentItem GetDraft(ContentItem item) {
            ContentItem draft;
            if (!_cache.TryGetValue(item.Id, out draft) || !draft.As<VersionInfoPart>().Draft) {
                draft = FindDraftFor(item.Id) ?? NewVersion(item);
                EnsureDraft(draft);

                if (draft != null) {
                    _cache[draft.Id] = draft;
                }
            }

            return draft;
        }

        public void ClearDraft(ContentItem item) {
            var latestVersion = item.Record.Versions.SingleOrDefault(x => x.Latest);
            var publishedVersion = item.Record.Versions.SingleOrDefault(x => x.Published);

            if (latestVersion != null && !latestVersion.Published) {
                latestVersion.Latest = false;

                if (publishedVersion != null) {
                    publishedVersion.Latest = true;
                }
            }

            // We have to ensure that the only draft (if exists) has been unmarked as draft
            var currentDraft = FindDraftFor(item.Id);
            if (currentDraft != null) {
                currentDraft.As<VersionInfoPart>().Draft = false;
            }
        }

        protected ContentItem FindDraftFor(int id) {
            // There can be only one Draft-marked item for a given content item
            var item = _infoPartRepository
                .Fetch(x => x.ContentItemVersionRecord != null && x.ContentItemRecord.Id == id && x.Draft)
                .OrderBy(x => x.Id)
                .SingleOrDefault();

            if (item != null)
            {
                try
                {
                    return _content.Get(-1, VersionOptions.VersionRecord(item.ContentItemVersionRecord.Id));
                }
                catch (Exception ex)
                {
                    // If we went here, something really bad happened (db is corrupted?).
                    // Marking the draft found as false, so it won't appear in the subsequent queries.
                    Logger.Error(ex, "Error when searching for draft for menu item {0}", item.Id);
                    item.Draft = false;
                    _infoPartRepository.Update(item);
                    _infoPartRepository.Flush();
                }
            }

            return null;
        }

        public IContent GetCurrent(IContent item)
        {
            return item.ContentItem.VersionRecord.Published 
                ? item 
                : _content.Get(item.Id, VersionOptions.Published);
        }

        public ContentItem GetVersion(int id, int versionNumber) {
            return _content.Get(id, VersionOptions.Number(versionNumber));
        }

        public bool HasRemovedVersion(int id)
        {
            return GetQueryable().Any(r => r.Removed && r.ContentItemRecord.Id == id);
        }

        public bool HasDraftVersion(int id)
        {
            return GetQueryable().Any(r => r.Draft && r.ContentItemRecord.Id == id);
        }

        public bool HasPublishedVersion(int id)
        {
            return GetQueryable().Any(r => r.ContentItemVersionRecord.Published && r.ContentItemRecord.Id == id);
        }

        public bool HasLatestVersion(int id)
        {
            return GetQueryable().Any(r => r.ContentItemVersionRecord.Latest && r.ContentItemRecord.Id == id);
        }

        public void PublishVersion(ContentItem item) {
            Publish(item);

            if (item.Is<VersionInfoPart>()) {
                item.As<VersionInfoPart>().Removed = false;
                item.As<VersionInfoPart>().Draft = false;
            }
        }

        private void Publish(ContentItem contentItem)
        {
            if (contentItem.VersionRecord.Published)
            {
                return;
            }
            // create a context for the item and it's previous published record
            var previous = _contentItemVersionRepository.Table.SingleOrDefault(r => r.ContentItemRecord == contentItem.Record && r.Published);
            var context = new PublishContentContext(contentItem, previous);

            // invoke handlers to acquire state, or at least establish lazy loading callbacks
            Handlers.Invoke(handler => handler.Publishing(context), Logger);

            if (previous != null)
            {
                previous.Published = false;
            }

            contentItem.VersionRecord.Published = true;

            Handlers.Invoke(handler => handler.Published(context), Logger);
        }

        public void RemoveVersion(ContentItem item) {
            _content.Remove(item);

            if (item.Is<VersionInfoPart>()) {
                item.As<VersionInfoPart>().Removed = true;
                item.As<VersionInfoPart>().Draft = false;
            }
        }

        public void SetLatest(ContentItem item) {
            var latestVersion = item.Record.Versions.SingleOrDefault(x => x.Latest);

            if (latestVersion != null) {
                latestVersion.Latest = false;
            }

            item.VersionRecord.Latest = true;
        }

        public IQueryable<VersionInfoPartRecord> GetQueryable()
        {
            return _infoPartRepository.Table;
        }

        public void EnsureDraft(ContentItem item) {
            var draftVersion = _infoPartRepository
                .Fetch(i => i.ContentItemRecord.Id == item.Id && i.Draft)
                .SingleOrDefault();

            if (draftVersion != null) {
                draftVersion.Draft = false;
            }

            item.As<VersionInfoPart>().Draft = true;
        }
    }
}