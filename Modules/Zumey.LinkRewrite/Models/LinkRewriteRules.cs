using System.Collections.Generic;

namespace Zumey.LinkRewrite.Models
{

    public class LinkRewriteRuleCollection : List<LinkRewriteRule>
    {
        public LinkRewriteRuleCollection(bool enabled)
        {
            Enabled = enabled;
        }

        public bool Enabled { get; set; }
    }

    public class LinkRewriteRule
    {
        public LinkRewriteRule()
        { 
        }

        public LinkRewriteRule(string pattern, string target)
        {
            Pattern = pattern;
            Target = target;
        }

        public string Pattern { get; set; }
        public string Target { get; set; }
    }
}