using System;
using System.IO;
using System.Text;
using HtmlAgilityPack;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Services;
using Orchard.ContentManagement;
using Zumey.LinkRewrite.Models;
using Zumey.LinkRewrite.Services;
using System.Text.RegularExpressions;

namespace Zumey.LinkRewrite.Filters
{
    [OrchardFeature("Zumey.LinkRewrite")]
    public class LinkRewriteFilter : IHtmlFilter
    {
        private ILinkRewriteService _service;
        private LinkRewriteRuleCollection _rules;
        
        public LinkRewriteFilter(ILinkRewriteService service)
        {
            _service = service;
        }

        public string ProcessContent(string text, string flavor)
        {
            if (!string.IsNullOrEmpty(text))
            {
                _rules = _service.GetRewriteRules();
                if (_rules.Enabled)
                {
                    switch (flavor)
                    {
                        case "html":
                            text = ProcessHtml(text);
                            break;
                        case "text":
                            text = ProcessText(text);
                            break;
                    }

                }
            }
            return text;
        }

        private string ProcessHtml(string text)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(text);
            ProcessNodes(doc, "//a[@href]", "href");
            ProcessNodes(doc, "//img[@src]", "src");

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                doc.Save(writer);
                text = sb.ToString();
            }
            return text;
        }

        private string ProcessText(string text)
        {
            return Rewrite(text);
        }

        private void ProcessNodes(HtmlDocument doc, string xPath, string attr)
        {
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(xPath);
            if (nodes != null)
            {
                foreach (HtmlNode node in nodes)
                {
                    Rewrite(node.Attributes[attr]);
                }
            }
        }

        private void Rewrite(HtmlAttribute attr)
        {
            //foreach (LinkRewriteRule rule in _rules)
            //{
            //    attr.Value = Regex.Replace(attr.Value, rule.Pattern, rule.Target);
            //}
            attr.Value = Rewrite(attr.Value);
        }

        private string Rewrite(string value)
        {
            foreach (LinkRewriteRule rule in _rules)
            {
                value = Regex.Replace(value, rule.Pattern, rule.Target);
            }
            return value;
        }

    }
}