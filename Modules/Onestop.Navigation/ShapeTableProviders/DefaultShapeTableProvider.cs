using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Utility.Extensions;

namespace Onestop.Navigation.ShapeTableProviders
{
    public class DefaultShapeTableProvider : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Menu").OnDisplaying(
                displaying =>
                {
                    var menu = displaying.Shape;
                    string menuId = menu.ItemId;
                    string menuName = menu.MenuName;
                    string zoneName = menu.ZoneName;

                    if (!string.IsNullOrWhiteSpace(menuName))
                    {
                        menu.Classes.Add("menu-" + menuName.HtmlClassify());
                        menu.Classes.Add("menu");

                        menu.Metadata.Alternates.Add("Menu__" + Encode(menuName));
                        // Adding alternates for menus by name, eg. Menu.<menu_name>
                        // eg. Menu-main.cshtml
                        menu.Metadata.Alternates.Add("Menu__" + Encode(menuName));

                        if (!string.IsNullOrWhiteSpace(zoneName))
                        {
                            // Adding alternates for menus by zone and menu name, eg. Menu.<menu_name>-<zone_name>
                            // eg. Menu-main-Navigation.cshtml
                            menu.Metadata.Alternates.Add("Menu__" + Encode(menuName) + "__" + Encode(zoneName));
                        }
                    }
                    // Adding alternates for menu widgets, eg. Menu-<widget_Id>
                    // eg. Menu-123
                    if (menuId != null)
                    {
                        menu.Id = "menu-" + menuId.ToLowerInvariant();
                        menu.Metadata.Alternates.Add("Menu__" + menuId);
                    }
                });

            builder.Describe("MenuItem").OnDisplaying(
                displaying =>
                {
                    var menuItem = displaying.Shape;
                    var menuShape = menuItem.Menu;

                    if (menuShape != null)
                    {
                        string menuName = menuShape.MenuName;
                        string zoneName = menuShape.ZoneName;
                        string idHint = menuItem.IdHint;
                        string groupName = menuItem.Group;
                        int level = menuItem.Level;

                        // Adding alternates for menu item by name, ie. MenuItem-<menu_name>
                        // eg. MenuItem-main.cshtml
                        menuItem.Metadata.Alternates.Add("MenuItem__" + Encode(menuName));

                        // Adding alternates for menu item by level
                        // eg. MenuItem-level-1.cshtml
                        menuItem.Metadata.Alternates.Add("MenuItem__level__" + level);

                        // Adding alternates for menu item by name and level, ie. MenuItem-<menu_name>-level-<level>
                        // eg. MenuItem-main-level-1.cshtml
                        menuItem.Metadata.Alternates.Add("MenuItem__" + Encode(menuName) + "__level__" + level);

                        // Adding alternates for menu items by zone and menu name, ie. MenuItem-<menu_name>-<zone_name>
                        // eg. MenuItem-main-Navigation.cshtml
                        menuItem.Metadata.Alternates.Add("MenuItem__" + Encode(menuName) + "__" + zoneName);

                        // Adding alternates for menu items by zone, menu name and level, ie. MenuItem-<menu_name>-<zone_name>-level-<level>
                        // eg. MenuItem-main-Navigation-level-1.cshtml
                        menuItem.Metadata.Alternates.Add("MenuItem__" + Encode(menuName) + "__" + zoneName + "__level__" + level);

                        if (!string.IsNullOrWhiteSpace(groupName))
                        {
                            // Adding alternates for menu item by group name
                            // eg. MenuItem-group-somegroup.cshtml
                            menuItem.Metadata.Alternates.Add("MenuItem__group__" + Encode(groupName.ToSafeName()));

                            // Adding alternates for menu item by group name
                            // eg. MenuItem-main-group-somegroup.cshtml
                            menuItem.Metadata.Alternates.Add("MenuItem__" + Encode(menuName) + "__group__" + Encode(groupName.ToSafeName()));

                            // Adding alternates for menu items by zone, menu name and group name, ie. MenuItem-<menu_name>-<zone_name>-group-<somegroupname>
                            // eg. MenuItem-main-Navigation-group-somegroup.cshtml
                            menuItem.Metadata.Alternates.Add("MenuItem__" + Encode(menuName) + "__" + zoneName + "__group__" + Encode(groupName.ToSafeName()));

                            // Adding alternates for menu items by zone, menu name, level and group name, ie. MenuItem-<menu_name>-<zone_name>-level-<level>-group-<somegroupname>
                            // eg. MenuItem-main-Navigation-level-1-group-somegroup.cshtml
                            menuItem.Metadata.Alternates.Add("MenuItem__" + Encode(menuName) + "__" + zoneName + "__level__" + level + "__group__" + Encode(groupName.ToSafeName()));
                        }

                        if (!string.IsNullOrWhiteSpace(idHint))
                        {
                            // Adding alternates for menu item by name
                            // eg. MenuItem-named-somecoolitem.cshtml
                            menuItem.Metadata.Alternates.Add("MenuItem__named__" + Encode(idHint.ToSafeName()));

                            // Adding alternates for menu item by name
                            // eg. MenuItem-main-named-somecoolitem.cshtml
                            menuItem.Metadata.Alternates.Add("MenuItem__" + Encode(menuName) + "__named__" + Encode(idHint.ToSafeName()));

                            // Adding alternates for menu items by zone, menu name and name, ie. MenuItem-<menu_name>-<zone_name>-named-<somename>
                            // eg. MenuItem-main-Navigation-named-somecoolitem.cshtml
                            menuItem.Metadata.Alternates.Add("MenuItem__" + Encode(menuName) + "__" + zoneName + "__named__" + Encode(idHint.ToSafeName()));

                            // Adding alternates for menu items by zone, menu name, level and name, ie. MenuItem-<menu_name>-<zone_name>-level-<level>-named-<somename>
                            // eg. MenuItem-main-Navigation-level-1-named-somecoolitem.cshtml
                            menuItem.Metadata.Alternates.Add("MenuItem__" + Encode(menuName) + "__" + zoneName + "__level__" + level + "__named__" + Encode(idHint.ToSafeName()));
                        }
                    }
                });

            builder.Describe("MenuItemLink")
                .OnDisplaying(displaying =>
                    {
                        var shape = displaying.Shape;
                        var menuShape = shape.Menu;
                        string menuName = menuShape.MenuName;
                        string zoneName = menuShape.ZoneName;
                        int level = shape.Level;
                        string idHint = shape.IdHint;
                        string groupName = shape.Group;

                        shape.Metadata.Alternates.Add("MenuItemLink__" + Encode(menuName));

                        var content = shape.Content as IContent;
                        if (content == null) return;

                        string contentType = content.ContentItem.ContentType;
                        var contentEmpty = contentType == null;
                        var idHintEmpty = string.IsNullOrWhiteSpace(idHint);
                        var groupNameEmpty = string.IsNullOrWhiteSpace(groupName);

                        if (!contentEmpty) {
                            shape.Metadata.Alternates.Add("MenuItemLink__" + contentType);
                            shape.Metadata.Alternates.Add("MenuItemLink__" + Encode(menuShape.MenuName) + "__" + contentType);
                            shape.Metadata.Alternates.Add("MenuItemLink__" + contentType + "__level__" + level);
                            shape.Metadata.Alternates.Add("MenuItemLink__" + Encode(menuName) + "__" + contentType + "__level__" + level);
                            shape.Metadata.Alternates.Add("MenuItemLink__" + Encode(menuName) + "__" + contentType + "__" + zoneName);
                            shape.Metadata.Alternates.Add("MenuItemLink__" + Encode(menuName) + "__" + contentType + "__" + zoneName + "__level__" + level);
                        }

                        if (!groupNameEmpty) {
                            shape.Metadata.Alternates.Add("MenuItemLink__group__" + Encode(groupName.ToSafeName()));
                            shape.Metadata.Alternates.Add("MenuItemLink__" + Encode(menuName) + "__group__" + Encode(groupName.ToSafeName()));
                        }

                        if (!contentEmpty && !groupNameEmpty) {
                            shape.Metadata.Alternates.Add("MenuItemLink__" + contentType + "__group__" + Encode(groupName.ToSafeName()));
                            shape.Metadata.Alternates.Add("MenuItemLink__" + Encode(menuShape.MenuName) + "__" + contentType + "__group__" + Encode(groupName.ToSafeName()));
                            shape.Metadata.Alternates.Add("MenuItemLink__" + contentType + "__level__" + level + "__group__" + Encode(groupName.ToSafeName()));
                            shape.Metadata.Alternates.Add("MenuItemLink__" + Encode(menuName) + "__" + contentType + "__level__" + level + "__group__" + Encode(groupName.ToSafeName()));
                            shape.Metadata.Alternates.Add("MenuItemLink__" + Encode(menuName) + "__" + contentType + "__" + zoneName + "__group__" + Encode(groupName.ToSafeName()));
                            shape.Metadata.Alternates.Add("MenuItemLink__" + Encode(menuName) + "__" + contentType + "__" + zoneName + "__level__" + level + "__group__" + Encode(groupName.ToSafeName()));
                        }

                        if (!idHintEmpty) {
                            shape.Metadata.Alternates.Add("MenuItemLink__named__" + Encode(idHint.ToSafeName()));
                            shape.Metadata.Alternates.Add("MenuItemLink__" + Encode(menuName) + "__named__" + Encode(idHint.ToSafeName()));
                        }

                        if (!contentEmpty && !idHintEmpty) {
                            shape.Metadata.Alternates.Add("MenuItemLink__" + contentType + "__named__" + Encode(idHint.ToSafeName()));
                            shape.Metadata.Alternates.Add("MenuItemLink__" + Encode(menuShape.MenuName) + "__" + contentType + "__named__" + Encode(idHint.ToSafeName()));
                            shape.Metadata.Alternates.Add("MenuItemLink__" + contentType + "__level__" + level + "__named__" + Encode(idHint.ToSafeName()));
                            shape.Metadata.Alternates.Add("MenuItemLink__" + Encode(menuName) + "__" + contentType + "__level__" + level + "__named__" + Encode(idHint.ToSafeName()));
                            shape.Metadata.Alternates.Add("MenuItemLink__" + Encode(menuName) + "__" + contentType + "__" + zoneName + "__named__" + Encode(idHint.ToSafeName()));
                            shape.Metadata.Alternates.Add("MenuItemLink__" + Encode(menuName) + "__" + contentType + "__" + zoneName + "__level__" + level + "__named__" + Encode(idHint.ToSafeName()));
                        }


                    });
        }

        private string Encode(string alternateElement)
        {
            return alternateElement.Replace("-", "__").Replace(".", "_");
        }
    }
}