using System;
using System.Collections.Generic;
using Orchard;

namespace Onestop.Patterns.Services
{
    public interface IPatternService : IDependency
    {
        /// <summary>
        /// Matches a given text against a given wildcard pattern.
        /// </summary>
        /// <param name="text">Text to match.</param>
        /// <param name="pattern">Pattern to match against.</param>
        /// <returns>True if match is found or false otherwise.</returns>
        bool TryMatch(string text, string pattern);

        /// <summary>
        /// Matches a given text against a given wildcard pattern.
        /// </summary>
        /// <param name="text">Text to match.</param>
        /// <param name="pattern">Pattern to match against.</param>
        /// <param name="details">Object containing detailed match information.</param>
        /// <returns>True if match is found or false otherwise.</returns>
        bool TryMatch(string text, string pattern, out PatternMatch details);

        /// <summary>
        /// In a specified string, replaces all strings that match a pattern 
        /// with a specified replacement string.
        /// </summary>
        /// <param name="text">Text to match.</param>
        /// <param name="pattern">Pattern to match against.</param>
        /// <param name="replacement">Replacement string.</param>
        /// <returns>The replaced string.</returns>
        string Replace(string text, string pattern, string replacement);

        /// <summary>
        /// In a specified string, replaces all strings that match a pattern 
        /// with a string returned by the replacement function.
        /// </summary>
        /// <param name="text">Text to match.</param>
        /// <param name="pattern">Pattern to match against.</param>
        /// <param name="replacement">Replacement string.</param>
        /// <returns>The replaced string.</returns>
        string Replace(string text, string pattern, Func<string, string> replacement);

        /// <summary>
        /// In a specified string, replaces all strings that match a pattern 
        /// with a string returned by the replacement function.
        /// </summary>
        /// <param name="text">Text to match.</param>
        /// <param name="pattern">Pattern to match against.</param>
        /// <param name="replacement">Replacement string.</param>
        /// <returns>The replaced string.</returns>
        string Replace(string text, string pattern, Func<PatternMatch, string> replacement);

        /// <summary>
        /// Returns names of groups defined in a given pattern.
        /// </summary>
        /// <param name="pattern">Pattern to check.</param>
        /// <returns>List of group names. Does not include whitespace-only or groups without names.</returns>
        IEnumerable<string> GetGroups(string pattern);

        /// <summary>
        /// Validates a given pattern
        /// </summary>
        /// <param name="pattern">Pattern to validate.</param>
        /// <returns>True if pattern is valide, false otherwise.</returns>
        bool Validate(string pattern);
    }
}