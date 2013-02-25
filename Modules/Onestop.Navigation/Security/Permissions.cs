using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Onestop.Navigation.Security {
    /// <summary>
    /// The permissions.
    /// </summary>
    public class Permissions : IPermissionProvider {
        /// <summary>
        /// Allows deleting menus
        /// </summary>
        public static readonly Permission DeleteMenu = new Permission {
            Description = "Delete menus", 
            Name = "DeleteMenu" 
        };

        /// <summary>
        /// Allows creating menus.
        /// </summary>
        public static readonly Permission CreateMenu = new Permission {
            Description = "Create menus",
            Name = "CreateMenu"
        };

        /// <summary>
        /// Allows editing menus.
        /// </summary>
        public static readonly Permission EditMenu = new Permission {
            Description = "Edit menus",
            Name = "EditMenu",
            ImpliedBy = new[] { CreateMenu }
        };

        /// <summary>
        /// Allows editing menus.
        /// </summary>
        public static readonly Permission EditAdvancedMenuItemOptions = new Permission {
            Description = "Edit advanced menu item options",
            Name = "EditAdvancedMenuItemOptions",
        };

        /// <summary>
        /// Allows editing menu items.
        /// </summary>
        public static readonly Permission DeleteMenuItems = new Permission {
            Description = "Delete menus' items",
            Name = "DeleteMenuItems",
            ImpliedBy = new[] { DeleteMenu }
        };

        /// <summary>
        /// Allows editing menu items.
        /// </summary>
        public static readonly Permission CreateMenuItems = new Permission {
            Description = "Create menus' items",
            Name = "CreateMenuItems",
            ImpliedBy = new[] { CreateMenu, EditMenu }
        };

        /// <summary>
        /// Allows editing menu items.
        /// </summary>
        public static readonly Permission EditMenuItems = new Permission {
            Description = "Edit menus' items",
            Name = "EditMenuItems",
            ImpliedBy = new[] { EditMenu, CreateMenuItems }
        };

        /// <summary>
        /// Gets or sets Feature.
        /// </summary>
        public virtual Feature Feature { get; set; }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[]
                {
                    new PermissionStereotype {
                        Name = "Administrator", Permissions = new[] { CreateMenu, EditMenu, DeleteMenu, EditAdvancedMenuItemOptions } 
                    }, 
                    new PermissionStereotype {
                        Name = "Editor", Permissions = new[] { CreateMenu, EditMenu, DeleteMenu } 
                    }, 
                    new PermissionStereotype { Name = "Moderator" } 
                };
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                CreateMenu, CreateMenuItems, EditMenu, EditMenuItems, DeleteMenu, DeleteMenuItems, EditAdvancedMenuItemOptions
            };
        }
    }
}