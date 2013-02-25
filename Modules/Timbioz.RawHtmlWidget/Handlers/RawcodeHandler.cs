using Timbioz.RawHtmlWidget.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Timbioz.RawHtmlWidget.Handlers
{
    public class RawcodeHandler : ContentHandler
    {
        public RawcodeHandler(IRepository<RawcodeRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}

