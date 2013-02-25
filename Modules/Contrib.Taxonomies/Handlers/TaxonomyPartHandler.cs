using Contrib.Taxonomies.Services;
using JetBrains.Annotations;
using Contrib.Taxonomies.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Contrib.Taxonomies.Handlers {
    [UsedImplicitly]
    public class TaxonomyPartHandler : ContentHandler {

        public TaxonomyPartHandler(
            IRepository<TaxonomyPartRecord> repository,
            ITaxonomyService taxonomyService
            ) {

            Filters.Add(StorageFilter.For(repository));

            OnPublished<TaxonomyPart>(
                (context, part) => taxonomyService.CreateTermContentType(part));

            OnLoading<TaxonomyPart>( 
                (context, part) => 
                    part._terms.Loader(x => taxonomyService.GetTerms(part.Id))
                );
        }
    }
}