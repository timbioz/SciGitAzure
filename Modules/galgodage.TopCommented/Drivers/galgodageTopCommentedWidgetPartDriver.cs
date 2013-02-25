using System;
using System.Collections.Generic;
using System.Linq;
using galgodage.TopCommented.Models;
using galgodage.TopCommented.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Comments.Models;


namespace galgodage.TopCommented.Drivers {
    public class galgodageTopCommentedWidgetPartDriver : ContentPartDriver<galgodageTopCommentedWidgetPart> {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly Lazy<IEnumerable<IContentPartDriver>> _contentPartDrivers;
       

        public galgodageTopCommentedWidgetPartDriver(
            IContentManager contentManager, 
            IContentDefinitionManager contentDefinitionManager,
            Lazy<IEnumerable<IContentPartDriver>> contentPartDrivers)
        {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _contentPartDrivers = contentPartDrivers;
          
        }

        protected override DriverResult Display(galgodageTopCommentedWidgetPart part, string displayType, dynamic shapeHelper) {
            // retrieve all content types implementing a specific content part
            var contentTypes = _contentManager.GetContentTypeDefinitions()
                .Where(x => x.Parts.Any(p => p.PartDefinition.Name == part.ForContentPart))
                .Select(x => x.Name)
                .ToArray();

            var query =_contentManager.Query(VersionOptions.Published, contentTypes)
                .Join<CommonPartRecord>().List();

            //Code from galgodage

            Dictionary<ContentItem, int> voteList = new Dictionary<ContentItem, int>();
            foreach (var art in query)
            {

             var resultRecord=   _contentManager.Query<CommentPart, CommentPartRecord>()
                        .Where(c => c.CommentedOn == art.Id );
             if (resultRecord != null)
             {
                 if((int)resultRecord.Count()>0)
                 voteList.Add(art, (int)resultRecord.Count());
             }
            }

            //part.Count = voteList.Count;
            var items = (from k in voteList.Keys
                        orderby voteList[k] descending
                        select k).Take(part.Count);         
            
           
            var list = shapeHelper.List();
            list.AddRange(items.Select(bp => _contentManager.BuildDisplay(bp, "Summary")));
           
            return ContentShape(shapeHelper.Parts_galgodageTopCommentedWidget_List(ContentPart: part, ContentItems: list));


        }

        protected override DriverResult Editor(galgodageTopCommentedWidgetPart part, dynamic shapeHelper) {
            var viewModel = new galgodageTopCommentedWidgetViewModel {
                Part = part, 
                ContentPartNames = GetParts()
            };

            return ContentShape("Parts_galgodageTopCommented_Widget_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts.galgodageTopCommented.Widget", Model: viewModel, Prefix: Prefix));
        }

        protected override DriverResult Editor(galgodageTopCommentedWidgetPart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new galgodageTopCommentedWidgetViewModel {
                Part = part
            };

            updater.TryUpdateModel(viewModel, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

        protected override void Importing(galgodageTopCommentedWidgetPart part, ImportContentContext context) {
            part.ForContentPart = context.Attribute(part.PartDefinition.Name, "ForContentPart");
            part.Count = Int32.Parse(context.Attribute(part.PartDefinition.Name, "Count"));
            //part.OrderBy = context.Attribute(part.PartDefinition.Name, "OrderBy");
        }

        protected override void Exporting(galgodageTopCommentedWidgetPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("ForContentPart", part.ForContentPart);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Count", part.Count);
            //context.Element(part.PartDefinition.Name).SetAttributeValue("OrderBy", part.OrderBy);
        }

        private IEnumerable<string> GetParts() {
            // user-defined parts
            var userContentParts = _contentDefinitionManager
                .ListPartDefinitions()
                .Select(x => x.Name);

            // code-defined parts
            var codeDefinedParts = _contentPartDrivers.Value.SelectMany(d => d.GetPartInfo().Where(cpd => !userContentParts.Any(m => m == cpd.PartName))).Select(x => x.PartName);

            return userContentParts.Union(codeDefinedParts).OrderBy(x => x);
        }
    }
}