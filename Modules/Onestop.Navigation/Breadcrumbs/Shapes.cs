using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Extensions;
using Orchard.Utility.Extensions;
using Orchard.Widgets.Models;

namespace Onestop.Navigation.Breadcrumbs {
    [OrchardFeature("Onestop.Navigation.Breadcrumbs")]
    public class Shapes : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Breadcrumbs").OnDisplaying(
                displaying =>
                    {
                        var shape = displaying.Shape;
                        var crumbs = shape.Breadcrumbs as Services.Breadcrumbs;

                        if (crumbs != null) {
                            shape.Metadata.Alternates.Add("Breadcrumbs__" + Encode(crumbs.Context.Provider));

                            if (crumbs.Context.Content != null) {
                                shape.Metadata.Alternates.Add("Breadcrumbs__" + crumbs.Context.Content.Id);
                            }

                            // Breadcrumbs shape is a mutated widget shape
                            ContentItem contentItem = shape.ContentItem;
                            if (contentItem != null && contentItem.Is<WidgetPart>())
                            {
                                var widgetPart = contentItem.As<WidgetPart>();
                                var zoneName = widgetPart.Zone;
                                var layerName = widgetPart.LayerPart.Name;

                                displaying.ShapeMetadata.Alternates.Add("Breadcrumbs__zone__" + zoneName);
                                displaying.ShapeMetadata.Alternates.Add("Breadcrumbs__layer__" + layerName);

                                // using the technical name to add an alternate
                                if (!String.IsNullOrWhiteSpace(widgetPart.Name))
                                {
                                    displaying.ShapeMetadata.Alternates.Add("Breadcrumbs__named__" + widgetPart.Name);
                                }

                            }

                            // Adding alternates based on pattern match named groups
                            object groups;
                            if (crumbs.Context.Properties.TryGetValue("Groups", out groups))
                            {
                                foreach (var group in (IDictionary<string, object>)groups) {
                                    shape.Metadata.Alternates.Add("Breadcrumbs__group__" + group.Key);
                                    shape.Metadata.Alternates.Add("Breadcrumbs__group__" + group.Key + "__" + group.ToString().HtmlClassify().ToSafeName());
                                }
                            }
                        }
                    });
        }

        private string Encode(string alternateElement) {
            return alternateElement.Replace("-", "__").Replace(".", "_");
        }
    }
}