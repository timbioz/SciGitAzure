using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Zumey.LinkRewrite.Models
{
    public class LinkRewriteSettingsRecord : ContentPartRecord
    {
        public virtual string Rules { get; set; }
        public virtual bool Enabled { get; set; }

    }
}