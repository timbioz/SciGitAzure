using System;
using System.Collections.Generic;
using Contrib.Taxonomies.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Contrib.Taxonomies.Fields {
    /// <summary>
    /// This field has not state, as all terms are saved using <see cref="TermContentItem"/>
    /// </summary>
    public class TaxonomyField : ContentField {
        internal LazyField<IEnumerable<TermPart>> _terms { get; set; }

        public TaxonomyField() {
            _terms = new LazyField<IEnumerable<TermPart>>();
        }

        /// <summary>
        /// Gets the Terms associated with this field
        /// </summary>
        public IEnumerable<TermPart> Terms {
            get { return _terms.Value; }
        }
    }
}