using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Contrib.Taxonomies.Models;
using Contrib.Taxonomies.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Admin;

namespace Onestop.Navigation.Controllers {
	[ValidateInput(false)]
	[Admin]
    [OrchardFeature("Onestop.Navigation.TaxonomyOrder")]
	public class TaxonomyOrderAdminController : Controller {
		private readonly ITaxonomyService _taxonomyService;

		public TaxonomyOrderAdminController(
			IOrchardServices services,
			IShapeFactory shapeFactory,
			ITaxonomyService taxonomyService) {
			_taxonomyService = taxonomyService;
			Services = services;
			Logger = NullLogger.Instance;
			T = NullLocalizer.Instance;
			Shape = shapeFactory;
		}

		public IOrchardServices Services { get; set; }
		public ILogger Logger { get; set; }
		public Localizer T { get; set; }
		private dynamic Shape { get; set; }

		public ActionResult List(int termId, string returnUrl) {
			var viewModel = Shape.TaxonomyOrderList();

			var term = _taxonomyService.GetTerm(termId);
			var terms = default(IList<TermPart>);
			if (term != null) {
				viewModel.Name = term.Name;
				terms = _taxonomyService.GetContentItemsQuery(term).ForPart<TermPart>().List().ToList();
			}
			else {
				viewModel.Name = _taxonomyService.GetTaxonomy(termId).Name;
				terms = _taxonomyService.GetTerms(termId).ToList();
			}

			terms = TermPart.Sort(terms).ToList();

			viewModel.Term = term;
			viewModel.ReturnUrl = returnUrl;
			viewModel.ContentItems = terms;
			return View(viewModel);
		}

		[HttpPost]
		public ActionResult Reorder(IEnumerable<int> itemIds) {
			var firstId = itemIds.First();
			var firstTerm = _taxonomyService.GetTerm(firstId);
			var parent = firstTerm.Container;
			IDictionary<int, TermPart> terms;
			if (parent != null && parent.ContentItem.ContentType == "Taxonomy") {
				terms = _taxonomyService.GetTerms(parent.Id)
					.ToDictionary(t => t.Id, t => t);
			}
			else {
				terms = _taxonomyService.GetContentItemsQuery(parent.As<TermPart>()).ForPart<TermPart>()
				.List()
				.ToDictionary(t => t.Id, t => t);
			}

			var i = 1;

			foreach (var itemId in itemIds.Reverse()) {
				if (terms.ContainsKey(itemId)) {
					terms[itemId].Weight = i;
					i++;
				}
			}

			return new JsonResult();
		}
	}
}