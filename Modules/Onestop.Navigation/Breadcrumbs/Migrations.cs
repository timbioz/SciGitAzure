using Orchard.Core.Contents.Extensions;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Onestop.Navigation.Breadcrumbs
{
    [OrchardFeature("Onestop.Navigation.Breadcrumbs")]
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable("BreadcrumbablePartRecord",
                      table => table
                                   .ContentPartRecord()
                                   .Column<string>("Provider"));

            ContentDefinitionManager.AlterTypeDefinition("BreadcrumbsWidget",
                cfg => cfg
                    .WithPart("BreadcrumbsWidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .DisplayedAs("Breadcrumbs")
                    .WithSetting("Stereotype", "Widget")
                );
            ContentDefinitionManager.AlterPartDefinition("BreadcrumbablePart", p => p.Attachable());
            ContentDefinitionManager.AlterTypeDefinition("Page", p => p.WithPart("BreadcrumbablePart"));

            SchemaBuilder.CreateTable("BreadcrumbsSiteSettingsPartRecord",
                table => table.ContentPartRecord()
                              .Column<string>("DefaultProvider"));

            SchemaBuilder.CreateTable("RoutePatternRecord",
                          table => table
                                       .Column<int>("Id", c => c.PrimaryKey().Identity())
                                       .Column<int>("Priority", c => c.WithDefault(1))
                                       .Column<string>("Pattern", c => c.Unlimited())
                                       .Column<string>("Provider"));
            return 3;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.CreateTable("BreadcrumbsSiteSettingsPartRecord",
                table => table.ContentPartRecord()
                              .Column<string>("DefaultProvider"));

            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.CreateTable("RoutePatternRecord",
                                      table => table
                                                   .Column<int>("Id", c => c.PrimaryKey().Identity())
                                                   .Column<int>("Priority", c => c.WithDefault(1))
                                                   .Column<string>("Pattern", c => c.Unlimited())
                                                   .Column<string>("Provider"));
            return 3;
        }
    }
}