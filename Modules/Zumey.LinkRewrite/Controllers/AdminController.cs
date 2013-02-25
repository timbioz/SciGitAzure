using System;
using System.Web.Mvc;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Core.Contents.Controllers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Zumey.LinkRewrite.Models;
using Zumey.LinkRewrite.Services;
using Zumey.LinkRewrite.ViewModels;

namespace Zumey.LinkRewrite.Controllers
{
    [Admin]
    [OrchardFeature("Zumey.LinkRewrite")]
    public class AdminController : Controller
    {

        private readonly IOrchardServices _services;
        private readonly ILinkRewriteService _service;
        private readonly ISignals _signals;

        public AdminController(IOrchardServices services, ILinkRewriteService service, ISignals signals)
        {
            _services = services;
            _service = service;
            _signals = signals;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [HttpGet]
        public ActionResult Index()
        {
            var settings = _services.WorkContext.CurrentSite.As<LinkRewriteSettingsPart>();
            var viewModel = new LinkRewriteViewModel
            {
                Settings = settings
            };
            return View("Index", viewModel);
        }

        [FormValueRequired("submit")]
        [HttpPost, ActionName("Index")]
        public ActionResult IndexPost()
        {
            if (!_services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage link rewrite settings")))
                return new HttpUnauthorizedResult();

            var viewModel = new LinkRewriteViewModel
            {
                Settings = _services.WorkContext.CurrentSite.As<LinkRewriteSettingsPart>(),
            };

            if (TryUpdateModel(viewModel))
            {
                try
                {
                    _service.ValidateRewriteRules();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("RewriteRules", ex.Message);
                }
            }
            if (ModelState.IsValid)
            {
                _signals.Trigger(LinkRewriteService.LinkRewriteRulesUpdated);
                _services.Notifier.Add(NotifyType.Information, T("Link Rewrite Rules successfully updated."));
            }
            else
            {
                _services.TransactionManager.Cancel();
            }
            return Index();
        }
 
    }
}