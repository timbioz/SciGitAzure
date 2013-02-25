using System.Collections.Generic;
using System.Linq;
using Onestop.Navigation.Services;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Environment;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Security.Permissions;

namespace Onestop.Navigation.Security {
    public class MenuPermissions : IPermissionProvider {
        private static readonly Permission CreateMenuItems = new Permission { 
            Description = "Create '{0}' menu items",
            Name = "CreateMenu_{0}",
            ImpliedBy = new[] { Permissions.DeleteMenu }
        };

        private static readonly Permission DeleteMenu = new Permission {
            Description = "Delete '{0}' menu", 
            Name = "DeleteMenu_{0}", 
            ImpliedBy = new[] { Permissions.DeleteMenu }
        };

        private static readonly Permission EditMenu = new Permission {
            Description = "Edit '{0}' menu details",
            Name = "EditMenu_{0}",
            ImpliedBy = new[] { Permissions.EditMenu }
        };

        private static readonly Permission EditMenuItems = new Permission {
            Description = "Edit '{0}' menu items",
            Name = "EditMenuItems_{0}",
            ImpliedBy = new[] { Permissions.EditMenu, Permissions.CreateMenuItems }
        };

        private static readonly Permission DeleteMenuItems = new Permission {
            Description = "Delete '{0}' menu items",
            Name = "DeleteMenuItems_{0}",
            ImpliedBy = new[] { Permissions.DeleteMenu, Permissions.DeleteMenuItems }
        };

        private static readonly Permission EditAdvancedMenuItemOptions = new Permission {
            Description = "Edit advanced '{0}' menu items' options.",
            Name = "EditAdvancedMenuItemOptions_{0}",
            ImpliedBy = new[] { Permissions.EditAdvancedMenuItemOptions }
        };

        public static readonly Dictionary<string, Permission> PermissionTemplates = new Dictionary<string, Permission> {
            { Permissions.CreateMenuItems.Name, CreateMenuItems },
            { Permissions.DeleteMenu.Name, DeleteMenu },
            { Permissions.DeleteMenuItems.Name, DeleteMenuItems },
            { Permissions.EditMenu.Name, EditMenu },
            { Permissions.EditMenuItems.Name, EditMenuItems },
            { Permissions.EditAdvancedMenuItemOptions.Name, EditAdvancedMenuItemOptions }
        };

        private readonly Work<IMenuService> _menuService;

        public MenuPermissions(Work<IMenuService> menuService) {
            _menuService = menuService;
            T = NullLocalizer.Instance;
        }

        public virtual Feature Feature { get; set; }
        public Localizer T { get; set; }

        /// <summary>
        /// Returns a dynamic permission for a content type, based on a global content permission template
        /// </summary>
        /// <param name="permission">
        /// The permission.
        /// </param>
        /// <returns>
        /// Dynamic permission based on a given template or null if none found.
        /// </returns>
        public static Permission ConvertToDynamicPermission(Permission permission) {
            if (PermissionTemplates.ContainsKey(permission.Name)) {
                return PermissionTemplates[permission.Name];
            }

            return null;
        }

        /// <summary>
        /// Generates a permission dynamically for a content type
        /// </summary>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <param name="menu">
        /// The menu.
        /// </param>
        /// <returns>
        /// Dynamic permission based on a given menu.
        /// </returns>
        public static Permission CreateDynamicPermission(Permission template, IContent menu) {
            return new Permission {
                Name = string.Format(template.Name, menu.As<TitlePart>().Title),
                Description = string.Format(template.Description, menu.As<TitlePart>().Title),
                Category = menu.As<TitlePart>().Title,
                ImpliedBy = (template.ImpliedBy ?? new Permission[0]).Select(t => CreateDynamicPermission(t, menu))
            };
        }

        /// <summary>
        /// Gets the default permission stereotypes
        /// </summary>
        /// <returns>
        /// Collection of permission stereotypes
        /// </returns>
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }

        /// <summary>
        /// Gets the menu-level permissions.
        /// </summary>
        /// <returns>
        /// Collection of menu-level permissions.
        /// </returns>
        public IEnumerable<Permission> GetPermissions() {
            var menus = _menuService.Value.GetMenus();

            return menus.SelectMany(
                menu => PermissionTemplates.Values,
                (menu, permissionTemplate) => CreateDynamicPermission(permissionTemplate, menu));
        }
    }
}