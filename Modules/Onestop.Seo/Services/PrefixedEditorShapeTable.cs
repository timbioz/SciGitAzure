﻿using System.Linq;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment;

namespace Onestop.Seo.Services {
    public class PrefixedEditorShapeTable : IShapeTableProvider {
        private readonly Work<IPrefixedEditorManager> _prefixedEditorManagerWork;

        public PrefixedEditorShapeTable(Work<IPrefixedEditorManager> prefixedEditorManagerWork) {
            _prefixedEditorManagerWork = prefixedEditorManagerWork;
        }

        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("EditorTemplate")
                .OnDisplaying(displaying => {
                    var shape = displaying.Shape;

                    if (shape["ContentItem"] == null) return; // This isn't a content item's shape, we can't help you.

                    int itemId = shape.ContentItem.Id;

                    if (!_prefixedEditorManagerWork.Value.ItemIds.Contains(itemId)) return;

                    shape.Prefix = PrefixedEditorManager.AttachPrefixToPrefix(itemId, shape.Prefix);
                });
        }
    }
}