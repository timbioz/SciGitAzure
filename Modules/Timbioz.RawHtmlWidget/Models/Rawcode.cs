using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Timbioz.RawHtmlWidget.Models
{
    public class RawcodeRecord : ContentPartRecord
    {
        public virtual string Html { get; set; }
        public virtual string Js { get; set; }
    }

    public class RawcodePart : ContentPart<RawcodeRecord>
    {

        public string Html
        {
            get { return Record.Html; }
            set { Record.Html = value; }
        }


        public string Js
        {
            get { return Record.Js; }
            set { Record.Js = value; }
        }
    }
}