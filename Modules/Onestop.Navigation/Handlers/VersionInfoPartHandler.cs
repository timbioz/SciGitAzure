using Onestop.Navigation.Models;
using JetBrains.Annotations;
using Orchard;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Security;

namespace Onestop.Navigation.Handlers {
    [UsedImplicitly]
    public class VersionInfoPartHandler : ContentHandler {
        private readonly IOrchardServices _services;

        public VersionInfoPartHandler(IRepository<VersionInfoPartRecord> repository, IOrchardServices services) {
            _services = services;
            Filters.Add(StorageFilter.For(repository));

            OnActivated<VersionInfoPart>(PropertySetHandlers);
            OnCreating<VersionInfoPart>((context, part) => SetInitialValues(part));
            OnLoading<VersionInfoPart>((context, part) => LazyLoadHandlers(part));
            OnVersioning<VersionInfoPart>((context, part, newVersionPart) => LazyLoadHandlers(newVersionPart));
            OnPublished<VersionInfoPart>(UpdatePublishedValues);
            OnVersioned<VersionInfoPart>(SetNewVersionValues);
        }

        private void UpdatePublishedValues(PublishContentContext context, VersionInfoPart part) {
            part.Draft = false;
            part.Removed = false;
        }

        public Localizer T { get; set; }

        protected void SetInitialValues(VersionInfoPart part)
        {
            part.Author = _services.WorkContext.CurrentUser;
            part.Draft = true;
            part.Removed = false;
        }

        protected void SetNewVersionValues(VersionContentContext context, VersionInfoPart part, VersionInfoPart newVersionPart)
        {
            newVersionPart.Draft = true;
            newVersionPart.Removed = false;
        }

        protected void LazyLoadHandlers(VersionInfoPart part) {
            part.AuthorField.Loader(() => part.Record != null ? _services.ContentManager.Get<IUser>(part.Record.Author) : null);
        }

        protected static void PropertySetHandlers(ActivatedContentContext context, VersionInfoPart part) {
            part.AuthorField.Setter(
                user => {
                    part.Record.Author = user == null ? 0 : user.ContentItem.Id;
                    return user;
                });

            if (part.AuthorField.Value != null) {
                part.AuthorField.Value = part.AuthorField.Value;
            }
        }
    }
}
