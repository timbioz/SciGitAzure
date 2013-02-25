using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Onestop.Navigation.Breadcrumbs.Models;
using Onestop.Navigation.Breadcrumbs.Services;
using Onestop.Navigation.Breadcrumbs.ViewModels;
using Onestop.Patterns.Services;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Onestop.Navigation.Controllers
{
    [OrchardFeature("Onestop.Navigation.Breadcrumbs")]
    [Admin]
    public class BreadcrumbsAdminController : Controller
    {
        private readonly IOrchardServices _services;
        private readonly IPatternService _patterns;
        private readonly INotifier _notifier;
        private readonly IBreadcrumbsService _breadcrumbs;

        public BreadcrumbsAdminController(
            IOrchardServices services, 
            IPatternService patterns, 
            INotifier notifier, 
            IBreadcrumbsService breadcrumbs)
        {
            _services = services;
            _patterns = patterns;
            _notifier = notifier;
            _breadcrumbs = breadcrumbs;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [HttpGet]
        public ActionResult Index()
        {
            var model = new BreadcrumbsIndexViewModel()
                            {
                                Matches = new Dictionary<string, PatternMatch>(),
                                Patterns = _breadcrumbs.GetPatterns(),
                                Providers = _breadcrumbs.GetProviderDescriptors()
                            };
            return View(model);
        }

        [HttpPost]
        [ActionName("Index")]
        public ActionResult IndexPOST(BreadcrumbsIndexViewModel model)
        {
            model.Patterns = _breadcrumbs.GetPatterns();
            model.Providers = _breadcrumbs.GetProviderDescriptors();
            foreach (var pattern in model.Patterns)
            {
                PatternMatch match;
                _patterns.TryMatch(model.TestString, pattern.Pattern, out match);
                model.Matches[pattern.Pattern] = match;
            }

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePOST(int id, string returnUrl)
        {
            if (!_services.Authorizer.Authorize(StandardPermissions.SiteOwner))
                return new HttpUnauthorizedResult();

            _breadcrumbs.DeletePattern(id);
            _notifier.Information(T("Entry deleted successfully!"));

            return Redirect(returnUrl);
        }

        [HttpPost]
        [ActionName("Add")]
        [ValidateAntiForgeryToken]
        public ActionResult AddPOST(string pattern, string provider, string returnUrl)
        {
            if (!_services.Authorizer.Authorize(StandardPermissions.SiteOwner))
                return new HttpUnauthorizedResult();

            if (!_patterns.Validate(pattern))
            {
                _notifier.Error(T("Pattern is not valid."));
                ModelState.AddModelError("Pattern", T("Pattern is not valid.").Text);
                return Redirect(returnUrl);
            }

            _breadcrumbs.AddPattern(pattern, provider);
            _notifier.Information(T("Entry created successfully!"));

            return Redirect(returnUrl);
        }

        [HttpPost]
        [ActionName("MoveUp")]
        [ValidateAntiForgeryToken]
        public ActionResult MoveUp(int id)
        {
            return null;
        }

        [HttpPost]
        [ActionName("MoveUp")]
        [ValidateAntiForgeryToken]
        public ActionResult MoveDown(int id)
        {
            return null;
        }
    }
}