using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Orchard.Caching;
using Orchard.Environment.Extensions;
using Orchard.Logging;

namespace Onestop.Patterns.Services
{
    [OrchardFeature("Onestop.Patterns")]
    public class PatternService : IPatternService
    {
        private readonly ICacheManager _cache;
        public PatternService(ICacheManager cache)
        {
            _cache = cache;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        /// <summary>
        /// Matches a given text against a given wildcard pattern.
        /// </summary>
        /// <param name="text">Text to match.</param>
        /// <param name="pattern">Pattern to match against.</param>
        /// <returns>True if match is found or false otherwise.</returns>
        public bool TryMatch(string text, string pattern)
        {
            PatternMatch details;
            return TryMatch(text, pattern, out details);
        }

        /// <summary>
        /// Matches a given text against a given wildcard pattern.
        /// </summary>
        /// <param name="text">Text to match.</param>
        /// <param name="pattern">Pattern to match against.</param>
        /// <param name="details">Object containing detailed match information.</param>
        /// <returns>True if match is found or false otherwise.</returns>
        public bool TryMatch(string text, string pattern, out PatternMatch details)
        {
            try
            {
                pattern = "{^}" + pattern + "{$}";
                var cached = _cache.Get(
                    "Onestop.Patterns:" + pattern,
                    ctx => new Tuple<IEnumerable<string>, Regex>(GetGroups(pattern), Convert(pattern)));

                var match = cached.Item2.Match(text);
                details = new PatternMatch
                {
                    Groups = cached.Item1.Select(g => new KeyValuePair<string, string>(g, match.Groups[g].Value)),
                    BaseExpression = cached.Item2,
                    IsMatch = match.Success
                };

                return details.IsMatch;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when trying to match pattern {0}", pattern);
                details = null;
                return false;
            }

        }

        /// <summary>
        /// In a specified string, replaces all strings that match a pattern 
        /// with a specified replacement string.
        /// </summary>
        /// <param name="text">Text to match.</param>
        /// <param name="pattern">Pattern to match against.</param>
        /// <param name="replacement">Replacement string.</param>
        /// <returns>The replaced string.</returns>
        public string Replace(string text, string pattern, string replacement)
        {
            try
            {
                var cached = _cache.Get(
                    "Onestop.Patterns:" + pattern,
                    ctx => new Tuple<IEnumerable<string>, Regex>(GetGroups(pattern), Convert(pattern)));

                return cached.Item2.Replace(text, replacement);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when trying to replace pattern {0}", pattern);
                return text;
            }
        }

        /// <summary>
        /// In a specified string, replaces all strings that match a pattern 
        /// with a string returned by the replacement function.
        /// </summary>
        /// <param name="text">Text to match.</param>
        /// <param name="pattern">Pattern to match against.</param>
        /// <param name="replacement">Replacement string.</param>
        /// <returns>The replaced string.</returns>
        public string Replace(string text, string pattern, Func<string, string> replacement)
        {
            try
            {
                var cached = _cache.Get(
                    "Onestop.Patterns:" + pattern,
                    ctx => new Tuple<IEnumerable<string>, Regex>(GetGroups(pattern), Convert(pattern)));

                return cached.Item2.Replace(text, match => replacement(match.ToString()));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when trying to replace pattern {0}", pattern);
                return text;
            }
        }

        public string Replace(string text, string pattern, Func<PatternMatch, string> replacement)
        {
            try
            {
                var cached = _cache.Get(
                    "Onestop.Patterns:" + pattern,
                    ctx => new Tuple<IEnumerable<string>, Regex>(GetGroups(pattern), Convert(pattern)));

                return cached.Item2.Replace(text, match =>
                {
                    var details = new PatternMatch
                    {
                        Groups = cached.Item1.Select(g => new KeyValuePair<string, string>(g, match.Groups[g].Value)),
                        BaseExpression = cached.Item2,
                        IsMatch = match.Success
                    };

                    return replacement(details);
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when trying to replace pattern {0}", pattern);
                return text;
            }
        }

        private static string EscapeSpecial(string text)
        {
            return text
                .Replace("\\", "\\\\")
                .Replace(".", "\\.")
                .Replace("[", "\\[")
                .Replace("]", "\\]")
                .Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace("|", "\\|")
                .Replace("/", "\\/")
                .Replace("*", "\\*")
                .Replace("$", "\\$")
                .Replace("^", "\\^")
                .Replace("?", "\\?");
        }
        private static string UnescapeSpecial(string text)
        {
            return text
                .Replace("\\\\", "\\")
                .Replace("\\.", ".")
                .Replace("\\[", "[")
                .Replace("\\]", "]")
                .Replace("\\(", "(")
                .Replace("\\)", ")")
                .Replace("\\|", "|")
                .Replace("\\/", "/")
                .Replace("\\*", "*")
                .Replace("\\$", "$")
                .Replace("\\^", "^")
                .Replace("\\?", "?");
        }

        private static Regex Convert(string pattern)
        {
            var groups = ExtractGroups(pattern);
            var regexPattern = groups.Aggregate(EscapeSpecial(pattern),
                (current, replacement) =>
                {
                    var newString = ReplaceWildcards(UnescapeSpecial(replacement.Item2.Trim()));
                    var groupName = replacement.Item1.Trim();

                    return current.Replace(
                        "{" + EscapeSpecial(replacement.Item1 + replacement.Item2) + "}",
                        !string.IsNullOrEmpty(groupName)
                            ? "(?'" + groupName + "'" + newString + ")"
                            : newString.Length == 1 ? newString : "(" + newString + ")");
                });

            return new Regex(regexPattern,
                RegexOptions.IgnoreCase |
                RegexOptions.ExplicitCapture |
                RegexOptions.Singleline |
                RegexOptions.IgnorePatternWhitespace);
        }

        public IEnumerable<string> GetGroups(string pattern)
        {
            return ExtractGroups(pattern)
                .Where(t => !string.IsNullOrWhiteSpace(t.Item1))
                .Select(t => t.Item1.Trim());
        }

        public bool Validate(string pattern)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pattern)) return false;

                var regex = Convert(pattern);
                regex.Match("");
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unexpected error when trying to validate pattern {0}", pattern);
                throw;
            }
        }

        private static IEnumerable<Tuple<string, string>> ExtractGroups(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var inGroup = false;
                var groupStart = 0;
                var groupContentStart = 0;
                var inGroupName = true;
                var groupName = "";

                for (var i = 0; i < text.Length; i++)
                {
                    var c = text[i];

                    if (c == '{')
                    {
                        if (i + 1 < text.Length && text[i + 1] == '{')
                        {
                            text = text.Substring(0, i) + text.Substring(i + 1);
                            continue;
                        }
                    }
                    else if (c == '}')
                    {
                        if (i + 1 < text.Length && text[i + 1] == '}')
                        {
                            text = text.Substring(0, i) + text.Substring(i + 1);
                            continue;
                        }
                    }

                    if (inGroup)
                    {
                        // Group name ends when first non-alphanumeric char is found
                        if (inGroupName && (!char.IsLetter(c) || char.IsWhiteSpace(c)))
                        {
                            inGroupName = false;
                            // If there is a pipe sign following, it's not a group name but text alternation
                            if (c == '|')
                            {
                                groupContentStart = groupStart + 1;
                                groupName = "";
                            }
                            else
                            {
                                groupContentStart = i;
                                groupName = text.Substring(groupStart + 1, i - groupStart - 1);
                            }
                        }

                        if (c == '}')
                        {
                            inGroup = false;
                            var groupContent = text.Substring(groupContentStart, i - groupContentStart);
                            yield return new Tuple<string, string>(
                                groupName,
                                groupContent);
                        }
                    }
                    else if (c == '{')
                    {
                        inGroup = true;
                        // Group name starts just after opening {, 
                        inGroupName = true;
                        groupStart = i;
                        groupName = "";
                    }
                }
            }
        }

        private static string ReplaceWildcards(string pattern)
        {
            var sb = new StringBuilder(pattern);

            while (sb.ToString().IndexOf("?*", StringComparison.Ordinal) != -1)
            {
                sb.Replace("?*", "*");
            }
            while (sb.ToString().IndexOf("**", StringComparison.Ordinal) != -1)
            {
                sb.Replace("**", "*");
            }
            while (sb.ToString().IndexOf(" ", StringComparison.Ordinal) != -1)
            {
                sb.Replace(" ", "\u0200");
            }
            // If the expressions are simple - change it to match any character (.)
            // * should match 0 or more characters
            if (sb.ToString().Equals("*"))
                return ".*";

            // + should match 1 or more characters
            else if (sb.ToString().Equals("+"))
                return ".+";

            // ? should match exactly one character
            else if (sb.ToString().Equals("?"))
                return ".";

            // Default quantifier (if none is specified) should not be greedy
            return string.IsNullOrWhiteSpace(pattern) ? ".+?" : sb.ToString();
        }
    }
}