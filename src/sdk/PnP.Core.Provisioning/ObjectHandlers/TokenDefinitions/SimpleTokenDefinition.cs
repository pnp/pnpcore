using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    /// <summary>
    /// Defines a provisioning engine Token. Make sure to only use the TokenContext property to execute queries in token methods.
    /// </summary>
    public abstract class SimpleTokenDefinition
    {
        protected string CacheValue;
        private readonly string[] _tokens;
        private readonly string[] _unescapedTokens;
        private readonly int _maximumTokenLength;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="token">token</param>
        public SimpleTokenDefinition(params string[] token)
        {
            _tokens = token;
            _unescapedTokens = GetUnescapedTokens(token);
            _maximumTokenLength = GetMaximumTokenLength(token);
        }

        /// <summary>
        /// Gets the amount of tokens hold by this token definition
        /// </summary>
        public int TokenCount
        {
            get => _tokens.Length;
        }

        /// <summary>
        /// Gets tokens
        /// </summary>
        /// <returns>Returns array string of tokens</returns>
        public string[] GetTokens()
        {
            return _tokens;
        }

        /// <summary>
        /// Gets the by <see cref="Regex.Unescape"/> processed tokens
        /// </summary>
        /// <returns>Returns array string of by <see cref="Regex.Unescape"/> processed tokens</returns>
        public IReadOnlyList<string> GetUnescapedTokens()
        {
            return _unescapedTokens;
        }

        /// <summary>
        /// Gets token length in integer
        /// </summary>
        /// <returns>token length in integer</returns>
        public int GetTokenLength()
        {
            return _maximumTokenLength;
        }

        /// <summary>
        /// abstract method
        /// </summary>
        /// <returns>Returns string</returns>
        public abstract string GetReplaceValue();

        /// <summary>
        /// Clears cache
        /// </summary>
        public void ClearCache()
        {
            CacheValue = null;
        }

        private static int GetMaximumTokenLength(IReadOnlyList<string> tokens)
        {
            var result = 0;

            for (var index = 0; index < tokens.Count; index++)
            {
                result = Math.Max(result, tokens[index].Length);
            }

            return result;
        }

        private static string[] GetUnescapedTokens(IReadOnlyList<string> tokens)
        {
            var result = new string[tokens.Count];

            for (var index = 0; index < tokens.Count; index++)
            {
                result[index] = Regex.Unescape(tokens[index]);
            }

            return result;
        }
    }
}