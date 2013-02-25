using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Onestop.Patterns.Services {
    public class PatternMatch {
        public PatternMatch() {
            Groups = Enumerable.Empty<KeyValuePair<string, string>>();
        }

        /// <summary>
        /// Is a successful match?
        /// </summary>
        public bool IsMatch { get; set; }

        /// <summary>
        /// List of groups and their matching substrings.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Groups { get; set; }

        /// <summary>
        /// Base regular expression.
        /// </summary>
        public Regex BaseExpression { get; set; }
    }
}