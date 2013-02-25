using System;
using System.Linq;
using System.Web.Routing;
using Onestop.Navigation.Models;
using Onestop.Navigation.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Extensions;
using Orchard.Core.Navigation.Models;
using Orchard.Data;
using Orchard.Localization;

namespace Onestop.Navigation.Handlers
{
    public class ExtendedMenuItemPartHandler : ContentHandler
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IVersionManager _versions;

        public ExtendedMenuItemPartHandler(
            IRepository<ExtendedMenuItemPartRecord> repository, 
            IContentDefinitionManager contentDefinitionManager, 
            IVersionManager versions)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _versions = versions;

            Filters.Add(StorageFilter.For(repository));

            OnActivated<ExtendedMenuItemPart>(PropertySetHandlers);
            OnLoading<ExtendedMenuItemPart>(LazyLoadHandlers);
            OnPublished<ExtendedMenuItemPart>(UpdateUnderlyingMenuPart);
            OnVersioned<ExtendedMenuItemPart>(SetNewVersionValues);
        }
        protected void SetNewVersionValues(VersionContentContext context, ExtendedMenuItemPart part, ExtendedMenuItemPart newVersionPart)
        {
            if (newVersionPart.Is<CommonPart>())
            {
                newVersionPart.As<CommonPart>().VersionModifiedUtc = part.As<CommonPart>().VersionModifiedUtc;
            }
        }

        private static void UpdateUnderlyingMenuPart(PublishContentContext context, ExtendedMenuItemPart part)
        {
            if (!part.Is<MenuPart>()) return;

            part.As<MenuPart>().MenuPosition = part.Position;
            part.As<MenuPart>().MenuText = part.Text;
        }

        public Localizer T { get; set; }

        protected override void Activating(ActivatingContentContext context) {
            if (!HasMenuItemStereotype(context.ContentType)) return;

            // Menu items have to be draftable
            var definition = _contentDefinitionManager.GetTypeDefinition(context.ContentType);

            // Need to auto-weld those parts as menu items may get created after installing the module...
            if (definition.Parts.All(p => p.PartDefinition.Name != "ExtendedMenuItemPart")) {
                context.Builder.Weld<ExtendedMenuItemPart>();
            }
            if (definition.Parts.All(p => p.PartDefinition.Name != "MenuPart")) {
                context.Builder.Weld<MenuPart>();
            }
            if (definition.Parts.All(p => p.PartDefinition.Name != "VersionInfoPart")) {
                context.Builder.Weld<VersionInfoPart>();
            }

            string draftable;
            if (!definition.Settings.TryGetValue("ContentTypeSettings.Draftable", out draftable) || draftable.Equals("false", StringComparison.OrdinalIgnoreCase)) {
                _contentDefinitionManager.AlterTypeDefinition(context.ContentType, cfg => cfg.Draftable());
            }
        }

        protected bool HasMenuItemStereotype(string typeName) {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(typeName);

            if (contentTypeDefinition != null)
                return contentTypeDefinition.Settings.ContainsKey("Stereotype") &&
                       contentTypeDefinition.Settings["Stereotype"] == "MenuItem";

            return false;
        }

        protected void LazyLoadHandlers(LoadContentContext context, ExtendedMenuItemPart part) {
            part.MenuVersionField.Loader(() => {
                if (part.Record != null && part.Record.MenuVersionRecord != null) {
                    // Adding consistency check - the version loaded must be a version of the currently set menu
                    var menu = part.As<MenuPart>().Menu;
                    if (menu != null && menu.Id == part.Record.MenuVersionRecord.ContentItemRecord.Id) {
                        return context.ContentManager.Get(-1, VersionOptions.VersionRecord(part.Record.MenuVersionRecord.Id));
                    }

                    // if the version is not consistent with the menu item - must be nullified
                    part.MenuVersion = null;
                }

                return null;
            });

            part.IsChangedField.Loader(() => _versions.HasDraftVersion(part.Id));
            part.IsCurrentField.Loader(() => !part.IsDraft && part.HasPosition);
            part.IsDraftField.Loader(() => part.As<IVersionAspect>().Draft && !part.HasPosition && !part.HasPublished);
            part.IsNewField.Loader(() => !part.HasPublished);
            part.IsPublishedField.Loader(() => part.ContentItem.VersionRecord != null && part.As<IVersionAspect>().Published);
            part.PublishedVersionField.Loader(() => _versions.GetCurrent(part.ContentItem));
            part.IsRemovedField.Loader(() => part.As<IVersionAspect>().Removed);
            part.HasPublishedField.Loader(() => _versions.HasPublishedVersion(part.Id));
            part.HasLatestField.Loader(() => _versions.HasLatestVersion(part.Id));

            // Forcing parent position to update if it hasn't been updated yet
            var parent = part.ParentPosition;
        }

        protected static void PropertySetHandlers(ActivatedContentContext context, ExtendedMenuItemPart part) {
            part.MenuVersionField.Setter(version => {
                // Adding consistency checks. Menu version set must be a version of the current menu
                var menu = part.As<MenuPart>().Menu;
                if (version == null || version.ContentItem == null || menu == null || menu.ContentItem.Id != version.ContentItem.Id) {
                    part.Record.MenuVersionRecord = null;
                }
                else {
                    part.Record.MenuVersionRecord = version.ContentItem.VersionRecord;
                }

                return version;
            });

            if (part.MenuVersionField.Value != null) {
                part.MenuVersionField.Value = part.MenuVersionField.Value;
            }
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (!HasMenuItemStereotype(context.ContentItem.ContentType)) {
                return;
            }

            context.Metadata.CreateRouteValues = new RouteValueDictionary {
                {"Area", "Onestop.Navigation"},
                {"Controller", "MenuAdmin"},
                {"Action", "CreateItem"},
                {"menuId", context.ContentItem.As<MenuPart>().Menu.Id },
                {"type", context.ContentItem.ContentType}
            };
            context.Metadata.EditorRouteValues = new RouteValueDictionary {
                {"Area", "Onestop.Navigation"},
                {"Controller", "MenuAdmin"},
                {"Action", "EditItem"},
                {"itemId", context.ContentItem.Id},
                {"menuId", context.ContentItem.As<MenuPart>().Menu.Id },
            };
            context.Metadata.AdminRouteValues = new RouteValueDictionary {
                {"Area", "Onestop.Navigation"},
                {"Controller", "MenuAdmin"},
                {"Action", "EditItem"},
                {"itemId", context.ContentItem.Id},
                {"menuId", context.ContentItem.As<MenuPart>().Menu.Id },
            };

            context.Metadata.RemoveRouteValues = new RouteValueDictionary {
                {"Area", "Onestop.Navigation"},
                {"Controller", "MenuAdmin"},
                {"Action", "DeleteItem"},
                {"itemId", context.ContentItem.Id},
                {"menuId", context.ContentItem.As<MenuPart>().Menu.Id },
            };
        }
    }
}