using System;
using System.Collections.Generic;
using System.Data;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Zumey.LinkRewrite 
{

    public class Migrations : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("LinkRewriteSettingsRecord", table => table
                .ContentPartRecord()
                .Column("Rules", DbType.String)
                .Column("Enabled", DbType.Boolean)
			);

            return 1;
        }

    }
}