using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Handles methods for token parser
    /// </summary>
    internal class SimpleTokenParser
    {
        private List<SimpleTokenDefinition> _tokens = new List<SimpleTokenDefinition>();

        private readonly Dictionary<string, string> _tokenDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        private static readonly Regex ReToken = new Regex(@"(?:(\{(?:\1??[^{]*?\})))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ReTokenFallback = new Regex(@"\{.*?\}", RegexOptions.Compiled);
        private static readonly Regex ReGuid = new Regex("(?<guid>\\{\\S{8}-\\S{4}-\\S{4}-\\S{4}-\\S{12}?\\})", RegexOptions.Compiled);
        private static readonly char[] TokenChars = { '{', '~' };

        /// <summary>
        /// adds token definition
        /// </summary>
        /// <param name="tokenDefinition">A TokenDefinition object</param>
        public void AddToken(SimpleTokenDefinition tokenDefinition)
        {
            _tokens.Add(tokenDefinition);
            AddToTokenCache(tokenDefinition);

            _tokens = _tokens.OrderByDescending(d => d.GetTokenLength()).ToList();
        }

        /// <summary>
        /// Parses the string
        /// </summary>
        /// <param name="input">input string to parse</param>
        /// <returns>Returns parsed string</returns>
        public string ParseString(string input)
        {
            return ParseString(input, null);
        }

        /// <summary>
        /// Parses given string
        /// </summary>
        /// <param name="input">input string</param>
        /// <param name="tokensToSkip">array of tokens to skip</param>
        /// <returns>Returns parsed string</returns>
        public string ParseString(string input, params string[] tokensToSkip)
        {
            if (string.IsNullOrWhiteSpace(input) || input.IndexOfAny(TokenChars) == -1)
            {
                return input;
            }

            if (_tokenDictionary.TryGetValue(input, out string directMatch))
            {
                return directMatch;
            }

            string output = input;
            bool hasMatch = false;

            do
            {
                hasMatch = false;
                output = ReToken.Replace(output, match =>
                {
                    string tokenString = match.Groups[0].Value;

                    if (!_tokenDictionary.TryGetValue(tokenString, out string val))
                    {
                        return tokenString;
                    }

                    hasMatch = true;
                    return val;
                });
            } while (hasMatch && input != output);

            if (hasMatch)
            {
                return output;
            }

            var fallbackMatches = ReTokenFallback.Matches(output);
            if (fallbackMatches.Count == 0)
            {
                return output;
            }

            bool needFallback = false;
            foreach (Match match in fallbackMatches)
            {
                if (!ReGuid.IsMatch(match.Value))
                {
                    needFallback = true;
                }
            }

            if (!needFallback)
            {
                return output;
            }

            foreach (var pair in _tokenDictionary)
            {
                int idx = output.IndexOf(pair.Key, StringComparison.CurrentCultureIgnoreCase);
                if (idx != -1)
                {
                    output = output.Remove(idx, pair.Key.Length).Insert(idx, pair.Value);
                }

                if (!ReTokenFallback.IsMatch(output))
                {
                    break;
                }
            }

            return output;
        }

        private void AddToTokenCache(SimpleTokenDefinition definition)
        {
            IReadOnlyList<string> tokens = definition.GetUnescapedTokens();
            for (var index = 0; index < tokens.Count; index++)
            {
                _tokenDictionary[tokens[index]] = definition.GetReplaceValue();
            }
        }
    }
}
