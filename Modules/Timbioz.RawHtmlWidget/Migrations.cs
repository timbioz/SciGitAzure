using System;
using System.Collections.Generic;
using System.Data;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Timbioz.RawHtmlWidget.Models;

namespace Timbioz.RawHtmlWidget {
    public class Migrations : DataMigrationImpl {

        public int Create() {
			// Creating table RawcodeRecord
			SchemaBuilder.CreateTable("RawcodeRecord", table => table
				.ContentPartRecord()
				.Column("Html", DbType.String, column => column.Unlimited())
                .Column("Js", DbType.String, column => column.Unlimited())
			);

            ContentDefinitionManager.AlterPartDefinition(
                typeof(RawcodePart).Name, cfg => cfg.Attachable());

            ContentDefinitionManager.AlterTypeDefinition("RawcodeWidget", cfg => cfg
                .WithPart("RawcodePart")
                .WithPart("WidgetPart")
                .WithPart("CommonPart")
                .WithSetting("Stereotype", "Widget"));

            return 1;
        }
    }
}