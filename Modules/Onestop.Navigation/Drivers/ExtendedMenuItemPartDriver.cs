using System;
using System.Collections.Generic;
using System.Linq;
using Onestop.Navigation.Models;
using Onestop.Navigation.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Navigation.Services;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security.Permissions;
using Orchard.UI.Notify;
using IMenuService = Onestop.Navigation.Services.IMenuService;

namespace Onestop.Navigation.Drivers {
    public class ExtendedMenuItemPartDriver : ContentPartDriver<ExtendedMenuItemPart> {
        private const string TemplateName = "Parts/Menu.ExtendedMenuItem.Edit";

        private readonly INotifier _notifier;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;
        private readonly ITransactionManager _trans;
        private readonly IMenuManager _menuManager;
        private readonly IMenuService _service;

        public ExtendedMenuItemPartDriver(
            IEnumerable<IPermissionProvider> permissionProviders,
            INotifier notifier,
            ITransactionManager trans,
            IMenuManager menuManager, IMenuService service) {
            _permissionProviders = permissionProviders;
            _notifier = notifier;
            _trans = trans;
            _menuManager = menuManager;
            _service = service;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "ExtendedMenuItem"; }
        }

        protected override DriverResult Editor(ExtendedMenuItemPart part, dynamic shapeHelper) {
            var model = BuildEditorViewModel(part);
            var results = new List<DriverResult> {
                ContentShape("Parts_Menu_ExtendedMenuItem_Edit", () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix)),
                ContentShape("Button_SaveAndAdd", publishButton => publishButton),
                ContentShape("Button_ManageMenu", () => shapeHelper.Button_ManageMenu(Menu: part.Menu))
            };

            return Combined(results.ToArray());
        }

        protected override DriverResult Editor(ExtendedMenuItemPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = BuildEditorViewModel(part);

            if (!updater.TryUpdateModel(model, Prefix, null, null)) {
                _notifier.Error(T("Error during menu update!"));
                _trans.Cancel();
            }
            else {
                if(_service.GetMenuItems(part.Menu, VersionOptions.Latest)
                    .Where(i => i.Id != part.Id)
                    .Any(i => !string.IsNullOrWhiteSpace(i.As<ExtendedMenuItemPart>().TechnicalName) 
                              && i.As<ExtendedMenuItemPart>().TechnicalName.Equals(part.TechnicalName, StringComparison.OrdinalIgnoreCase)))
                {
                    updater.AddModelError(Prefix + ".Part.TechnicalName", T("This technical name is already in use. Type a different one."));
                    _trans.Cancel();
                }
                _notifier.Information(T("Menu updated successfully"));
            }

            return Editor(part, shapeHelper);
        }

        private ItemEditorViewModel BuildEditorViewModel(ExtendedMenuItemPart part) {
            var model = new ItemEditorViewModel {
                    MenuItemDescriptors = _menuManager.GetMenuItemTypes(),
                    Part = part,
                    VersionId = part.IsPublished ? part.ContentItem.VersionRecord.Id : 0,
                    Permissions = _permissionProviders
                        .SelectMany(p => p.GetPermissions()
                                          .Select(perm => {
                                              if (perm.Category == null) {
                                                  perm.Category = "Feature: " + p.Feature.Descriptor.Id;
                                              }
                                              return perm;
                                           }))
                        .GroupBy(p => p.Category ?? "")
                        .ToDictionary(g => g.Key, g => g.Select(p => p))
                };

            return model;
        }
    }
}