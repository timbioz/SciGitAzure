using System.Collections.Generic;
using Contrib.Taxonomies.Models;

namespace Contrib.Taxonomies.ViewModels {
    public class MergeTermViewModel {
        public IEnumerable<TermPart> Terms { get; set; }
        public int SelectedTermId { get; set; }

        public int TaxonomyId { get; set; }

        public TermPart[] TermsToMerge { get; set; }

        public bool ForcePublish { get; set; }
    }
}