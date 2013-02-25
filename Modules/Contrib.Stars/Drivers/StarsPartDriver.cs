using System.Linq;
using Contrib.Stars.Models;
using Contrib.Stars.Settings;
using Contrib.Voting.Models;
using Contrib.Voting.Services;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;

namespace Contrib.Stars.Drivers {
    public class StarsPartDriver : ContentPartDriver<StarsPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IVotingService _votingService;

        public StarsPartDriver(IOrchardServices orchardServices, IVotingService votingService) {
            _orchardServices = orchardServices;
            _votingService = votingService;
        }

        protected override DriverResult Display(StarsPart part, string displayType, dynamic shapeHelper) {
            if (!part.ShowStars)
                return null;

            return Combined(
                ContentShape(
                    "Parts_Stars",
                        () => shapeHelper.Parts_Stars(BuildStars(part))),
                ContentShape(
                    "Parts_Stars_SummaryAdmin",
                        () => shapeHelper.Parts_Stars_SummaryAdmin(BuildStars(part)))
                );
        }

        private StarsPart BuildStars(StarsPart part) {
            part.ResultValue = (_votingService.GetResult(part.ContentItem.Id, "average")
                ?? new ResultRecord()).Value;
                    
            // get the user's vote
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser != null) {
                var userRating = _votingService.Get(vote => vote.Username == currentUser.UserName && vote.ContentItemRecord == part.ContentItem.Record).FirstOrDefault();
                part.UserRating = userRating != null ? userRating.Value : 0;
            }

            return part;
        }
    }

    public class StarsPartHandler : ContentHandler {
        public StarsPartHandler() {
            OnInitializing<StarsPart>((context, part) => {
                part.ShowStars = part.Settings.GetModel<StarsTypePartSettings>().ShowStars;
                part.AllowAnonymousRatings = part.Settings.GetModel<StarsTypePartSettings>().AllowAnonymousRatings;
            });
        }
    }
}