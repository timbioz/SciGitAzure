using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Zumey.LinkRewrite.Models
{
    public class LinkRewriteSettingsPart : ContentPart<LinkRewriteSettingsRecord>
    {

        public string Rules
        {
            get { return Record.Rules; }
            set { Record.Rules = value; }
        }

        [Required]
        public bool Enabled
        {
            get { return Record.Enabled; }
            set { Record.Enabled = value; }
        }

    }
}