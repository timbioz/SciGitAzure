using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Onestop.Navigation {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            /* Menu item record */
            SchemaBuilder.CreateTable(
                "ExtendedMenuItemPartRecord",
                table => table
                    .ContentPartVersionRecord()
                    .Column<int>("MenuVersionRecord_id")
                    .Column<string>("Position", c => c.WithDefault("0"))
                    .Column<string>("ParentPosition")
                    .Column<string>("Classes", c => c.WithLength(1000))
                    .Column<bool>("DisplayText", c => c.WithDefault(true))
                    .Column<bool>("DisplayHref", c => c.WithDefault(true))
                    .Column<string>("Permission")
                    .Column<string>("CssId", c => c.WithLength(1000))
                    .Column<string>("DraftPosition", c => c.WithDefault("0"))
                    .Column<bool>("InNewWindow")
                    .Column<string>("Text", c => c.WithLength(1000))
                    .Column<string>("Url", c => c.WithLength(1000))
                    .Column<string>("TechnicalName", c => c.WithLength(256))
                    .Column<string>("GroupName", c => c.WithLength(256))
                    .Column<string>("Subtitle", c => c.WithLength(1000)));

            SchemaBuilder.CreateTable("OnestopMenuWidgetPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("Menu_id")
                    .Column<string>("Mode")
                    .Column<string>("RootNode")
                    .Column<bool>("WrapChildrenInDivs")
                    .Column<int>("Levels")
                    .Column<bool>("CutOrFlattenLower"));

            SchemaBuilder.CreateTable("ImageMenuItemPartRecord",
                table => table
                    .ContentPartVersionRecord()
                    .Column<string>("AlternateText")
                    .Column<string>("Class")
                    .Column<string>("Style")
                    .Column<string>("Alignment")
                    .Column<int>("Width")
                    .Column<int>("Height ")
                    .Column<string>("Url", c => c.WithLength(1000)));

            SchemaBuilder.CreateTable("VersionInfoPartRecord",
                table => table
                    .ContentPartVersionRecord()
                    .Column<int>("Author")
                    .Column<bool>("Removed", c => c.WithDefault(false))
                    .Column<bool>("Draft", c => c.WithDefault(false)));

            SchemaBuilder.CreateTable("AdminMenuItemRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Text")
                    .Column<string>("Position")
                    .Column<string>("ItemGroup")
                    .Column<string>("GroupPosition")
                    .Column<string>("Url"));   

            /* Remove the previous name of this part and alter parts' definitions */
            ContentDefinitionManager.AlterTypeDefinition(
                "MenuWidget",
                cfg => cfg
                    .WithPart("OnestopMenuWidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithPart("IdentityPart")
                    .RemovePart("MenuWidgetPart")
                    .DisplayedAs("Menu Widget")
                    .WithSetting("Stereotype", "Widget"));

            ContentDefinitionManager.AlterTypeDefinition("Menu", 
                cfg => cfg
                    .WithPart("IdentityPart")
                    .WithPart("VersionInfoPart"));      

            ContentDefinitionManager.AlterTypeDefinition("ImageMenuItem", 
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                    .WithPart("MenuItemPart")
                    .WithPart("MenuPart")
                    .WithPart("ImageMenuItemPart")
                    .DisplayedAs("Image Item")
                    .WithSetting("Description", "Represents an image menu item with a link.")
                    .WithSetting("Stereotype", "MenuItem"));

            SchemaBuilder.AlterTable("ExtendedMenuItemPartRecord", table => table.CreateIndex("PositionIndex", "Position"));
            SchemaBuilder.AlterTable("ExtendedMenuItemPartRecord", table => table.CreateIndex("ParentPositionIndex", "ParentPosition"));
            SchemaBuilder.AlterTable("ExtendedMenuItemPartRecord", table => table.CreateIndex("PositionItemIndex", "Position", "ContentItemRecord_id"));

            return 4;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.AlterTable("ExtendedMenuItemPartRecord", table => table.AddColumn<string>("GroupName", c => c.WithLength(256)));
            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.AlterTable("ExtendedMenuItemPartRecord", table => table.CreateIndex("PositionIndex", "Position"));
            SchemaBuilder.AlterTable("ExtendedMenuItemPartRecord", table => table.CreateIndex("PositionItemIndex", "Position", "ContentItemRecord_id"));
            return 3;
        }

        public int UpdateFrom3()
        {
            SchemaBuilder.AlterTable("ExtendedMenuItemPartRecord", table => table.AddColumn<string>("ParentPosition"));
            SchemaBuilder.AlterTable("ExtendedMenuItemPartRecord", table => table.CreateIndex("ParentPositionIndex", "ParentPosition"));
            return 4;
        }
    }
}