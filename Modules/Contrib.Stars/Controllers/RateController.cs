using System.Linq;
using System.Web.Mvc;
using Contrib.Stars.Models;
using Contrib.Voting.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Mvc.Extensions;

namespace Contrib.Stars.Controllers {
    public class RateController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly IVotingService _votingService;

        public RateController(IOrchardServices orchardServices, IContentManager contentManager, IVotingService votingService) {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _votingService = votingService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [HttpPost]
        public ActionResult Apply(int contentId, int rating, string returnUrl) {
            var content = _contentManager.Get(contentId);
            if (content == null || !content.Has<StarsPart>() || !content.As<StarsPart>().ShowStars)
                return this.RedirectLocal(returnUrl, "~/");

            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if ((currentUser == null && !content.As<StarsPart>().AllowAnonymousRatings) || rating < -1 || rating > 5 || rating == 0) // invalid and no-op (0)
                return this.RedirectLocal(returnUrl, "~/");

            //var userName = currentUser != null ? currentUser.UserName : T("Anonymous").Text; // info: could use a display name for the user
            if (currentUser != null) {
                var currentVote = _votingService.Get(vote => vote.Username == currentUser.UserName && vote.ContentItemRecord == content.Record).FirstOrDefault();
                if (rating == -1) { // clear
                    if (currentVote != null)
                        _votingService.RemoveVote(currentVote);
                }
                else { // vote!!
                    if (currentVote != null)
                        _votingService.ChangeVote(currentVote, rating);
                    else
                        _votingService.Vote(content, currentUser.UserName, HttpContext.Request.UserHostAddress, rating);
                }
            }
            else {
                var anonHostname = HttpContext.Request.UserHostAddress;
                if (!string.IsNullOrWhiteSpace(HttpContext.Request.Headers["X-Forwarded-For"]))
                    anonHostname += "-" + HttpContext.Request.Headers["X-Forwarded-For"];

                var currentVote = _votingService.Get(vote => vote.Username == "Anonymous" && vote.Hostname == anonHostname && vote.ContentItemRecord == content.Record).FirstOrDefault();
                if (rating > 0 && currentVote == null) // anonymous votes are only set once per anonHostname
                    _votingService.Vote(content, "Anonymous", anonHostname, rating);
            }

            return this.RedirectLocal(returnUrl, "~/");
        }
    }
}