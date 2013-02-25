using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace galgodage.TopCommented
{
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("galgodageTopCommentedWidgetPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("ForContentPart")
                    .Column<string>("OrderBy")
                    .Column<int>("Count")
                );

            ContentDefinitionManager.AlterTypeDefinition("galgodageTopCommented",
                cfg => cfg
                    .WithPart("galgodageTopCommentedWidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithSetting("Stereotype", "Widget")
                );

            ContentDefinitionManager.AlterTypeDefinition("galgodageTopCommented",
                cfg => cfg
                    .WithPart("galgodageTopCommentedWidgetPart")
                );
            return 1;
        }
    }
}