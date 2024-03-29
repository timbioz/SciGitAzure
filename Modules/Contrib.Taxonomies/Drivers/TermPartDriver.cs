﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Contrib.Taxonomies.Models;
using Contrib.Taxonomies.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Feeds;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Settings;
using Orchard.UI.Navigation;

namespace Contrib.Taxonomies.Drivers {
    public class TermPartDriver : ContentPartDriver<TermPart> {
        private readonly ITaxonomyService _taxonomyService;
        private readonly ISiteService _siteService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFeedManager _feedManager;
        private readonly IContentManager _contentManager;

        public TermPartDriver(
            ITaxonomyService taxonomyService,
            ISiteService siteService,
            IHttpContextAccessor httpContextAccessor,
            IFeedManager feedManager,
            IContentManager contentManager) {
            _taxonomyService = taxonomyService;
            _siteService = siteService;
            _httpContextAccessor = httpContextAccessor;
            _feedManager = feedManager;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        Localizer T { get; set; }

        protected override string Prefix { get { return "Term"; } }

        protected override DriverResult Display(TermPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_TermPart_Feed", () => {
                    
                    // generates a link to the RSS feed for this term
                    _feedManager.Register(part.Name, "rss", new RouteValueDictionary { { "term", part.Id } });
                    return null;
                }),
                ContentShape("Parts_TermPart", () => {
                    var pagerParameters = new PagerParameters();
                    
                    var httpContext = _httpContextAccessor.Current();
                    if (httpContext != null) {
                        pagerParameters.Page = Convert.ToInt32(httpContext.Request.QueryString["page"]);
                    }
                    
                    var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

                    var taxonomy = _taxonomyService.GetTaxonomy(part.TaxonomyId);
                    var totalItemCount = _taxonomyService.GetContentItemsCount(part);

                    // asign Taxonomy and Term to the content item shape (Content) in order to provide 
                    // alternates when those content items are displayed when they are listed on a term
                    var termContentItems = _taxonomyService.GetContentItems(part, pager.GetStartIndex(), pager.PageSize)
                        .Select(c => _contentManager.BuildDisplay(c, "Summary").Taxonomy(taxonomy).Term(part));

                    var list = shapeHelper.List();

                    list.AddRange(termContentItems);

                    var pagerShape = shapeHelper.Pager(pager)
                            .TotalItemCount(totalItemCount)
                            .Taxonomy(taxonomy)
                            .Term(part);

                    return shapeHelper.Parts_TermPart(ContentItems: list, Taxonomy: taxonomy, Pager: pagerShape);
                }));

        }
        protected override DriverResult Editor(TermPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Taxonomies_Term_Fields",
                    () => shapeHelper.EditorTemplate(TemplateName: "Parts/Taxonomies.Term.Fields", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(TermPart termPart, IUpdateModel updater, dynamic shapeHelper) {
            if (updater.TryUpdateModel(termPart, Prefix, null, null)) {
                var existing = _taxonomyService.GetTermByName(termPart.TaxonomyId, termPart.Name);
                if (existing != null && existing.Record != termPart.Record && existing.Container.ContentItem.Record == termPart.Container.ContentItem.Record) {
                    updater.AddModelError("Name", T("The term {0} already exists at this level", termPart.Name));
                }
            }

            return Editor(termPart, shapeHelper);
        }

        protected override void Exporting(TermPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Count", part.Record.Count);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Selectable", part.Record.Selectable);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Weight", part.Record.Weight);

            var taxonomy = _contentManager.Get(part.Record.TaxonomyId);
            var identity = _contentManager.GetItemMetadata(taxonomy).Identity.ToString();
            context.Element(part.PartDefinition.Name).SetAttributeValue("TaxonomyId", identity);

            var identityPaths = new List<string>();
            foreach(string pathPart in part.Record.Path.Split('/')) {
                if(String.IsNullOrEmpty(pathPart)) {
                    continue;
                }

                var parent = _contentManager.Get(Int32.Parse(pathPart));
                identityPaths.Add(_contentManager.GetItemMetadata(parent).Identity.ToString());
            }

            context.Element(part.PartDefinition.Name).SetAttributeValue("Path", String.Join(",", identityPaths.ToArray()));

        }

        protected override void Importing(TermPart part, ImportContentContext context) {
            part.Record.Count = Int32.Parse(context.Attribute(part.PartDefinition.Name, "Count"));
            part.Record.Selectable = Boolean.Parse(context.Attribute(part.PartDefinition.Name, "Selectable"));
            part.Record.Weight = Int32.Parse(context.Attribute(part.PartDefinition.Name, "Weight"));

            var identity = context.Attribute(part.PartDefinition.Name, "TaxonomyId");
            var contentItem = context.GetItemFromSession(identity);
            
            if (contentItem == null) {
                throw new OrchardException(T("Unknown taxonomy: {0}", identity));
            } 
            
            part.Record.TaxonomyId = contentItem.Id;


            part.Record.Path = "/";
            foreach(var identityPath in context.Attribute(part.PartDefinition.Name, "Path").Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries)) {
                var pathContentItem = context.GetItemFromSession(identityPath);
                part.Record.Path += pathContentItem.Id + "/";
            }
        }
    }
}