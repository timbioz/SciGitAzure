using JetBrains.Annotations;
using Onestop.Navigation.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Navigation.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Onestop.Navigation.Handlers {
    [UsedImplicitly]
    [OrchardSuppressDependency("Orchard.Core.Navigation.Handlers.MenuPartHandler")]
    public class MenuPartHandler : Orchard.Core.Navigation.Handlers.MenuPartHandler {
        public MenuPartHandler(
            IRepository<MenuPartRecord> menuPartRepository,
            IContentManager contentManager
            ) : base(menuPartRepository, contentManager) {

        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<ExtendedMenuItemPart>();

            if (part != null) {
                string stereotype;
                if (context.ContentItem.TypeDefinition.Settings.TryGetValue("Stereotype", out stereotype) && stereotype == "MenuItem") {
                    context.Metadata.DisplayText = !string.IsNullOrWhiteSpace(part.Text) ? part.Text : "<No text>";    
                }
            }
        }

    }
}