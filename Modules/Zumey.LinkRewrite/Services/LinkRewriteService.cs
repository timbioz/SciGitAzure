using System;
using System.Linq;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Zumey.LinkRewrite.Models;
using System.Text.RegularExpressions;
using Orchard.Localization;

namespace Zumey.LinkRewrite.Services
{
    public interface ILinkRewriteService : IDependency
    {
        LinkRewriteRuleCollection GetRewriteRules();
        void ValidateRewriteRules();
    }

    [OrchardFeature("Zumey.LinkRewrite")]
    public class LinkRewriteService : ILinkRewriteService
    {
        private readonly IRepository<LinkRewriteSettingsRecord> _rewriteRuleRepository;

        private readonly WorkContext _context;
        private readonly IContentManager _contentManager;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        internal static readonly string LinkRewriteRulesUpdated = "Zumey.LinkRewrite.LinkRewriteRulesUpdated";
        internal static readonly string LinkRewriteRulesCacheKey = "Zumey.LinkRewrite.LinkRewriteRulesCache";

        public LinkRewriteService(
            IWorkContextAccessor wca,
            IRepository<LinkRewriteSettingsRecord> rewriteRuleRepository,
            IContentManager contentManager,
            ICacheManager cacheManager,
            ISignals signals)
        {
            _context = wca.GetContext();
            _rewriteRuleRepository = rewriteRuleRepository;
            _contentManager = contentManager;
            _cacheManager = cacheManager;
            _signals = signals;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public LinkRewriteRuleCollection GetRewriteRules()
        {
            return _cacheManager.Get(LinkRewriteService.LinkRewriteRulesCacheKey, ctx =>
                {
                    ctx.Monitor(_signals.When(LinkRewriteService.LinkRewriteRulesUpdated));
                    return ParseRewriteRules();
                });
        }

        public void ValidateRewriteRules()
        {
            ParseRewriteRules();
        }

        private LinkRewriteRuleCollection ParseRewriteRules()
        {
            var settings = _context.CurrentSite.As<LinkRewriteSettingsPart>();
            LinkRewriteRuleCollection rules = new LinkRewriteRuleCollection(settings == null ? false : settings.Enabled);
            if (rules.Enabled)
            {
                string[] rawRules = settings.Rules.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                char[] delimiter = { '=', '>' };
                foreach (string rawRule in rawRules)
                {
                    string[] tokens = rawRule.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length == 2)
                    {
                        LinkRewriteRule rule = new LinkRewriteRule(tokens[0].Trim(), tokens[1].Trim());
                        if (ValidateRegEx(rule.Pattern))
                        {
                            rules.Add(rule);
                        }
                        else
                        {
                            rules.Clear();
                            throw new ArgumentException(T("The rewrite rule '{0}' is not a valid regular expression.", rule.Pattern).Text, "Pattern");
                        }
                    }
                    else
                    {
                        rules.Clear();
                        throw new ArgumentException(T("The rewrite rule '{0}' is not in the expected format of 'regex=>target'.", rawRule).Text, "Rule");
                    }
                }
            }
            return rules;
        }

        private bool ValidateRegEx(string pattern)
        {
            bool valid = !string.IsNullOrEmpty(pattern);
            try { Regex.Match(string.Empty, pattern); }
            catch (ArgumentException) { valid = false; }
            return valid;
        }

    }
}