using PnP.Core.Transformation.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PnP.Core.Transformation.SharePoint.Services
{
    /// <summary>
    /// Resolves tokens by their actual representation
    /// </summary>
    public static class TokenParser
    {
        /// <summary>
        /// Replaces the tokens in the provided input string with their values
        /// </summary>
        /// <param name="input">String with tokens</param>
        /// <param name="webPartData">Web part information holding all possible tokens for this web part</param>
        /// <returns>A string with tokens replaced by actual values</returns>
        public static string ReplaceTokens(string input, WebPartEntity webPartData)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            var tokenChars = new[] { '{' };
            if (string.IsNullOrEmpty(input) || input.IndexOfAny(tokenChars) == -1)
            {
                return input;
            }

            string origInput;
            do
            {
                origInput = input;
                foreach (var property in webPartData.Properties)
                {
                    if (property.Value != null)
                    {
                        var regex = new Regex($"{{{property.Key}}}", RegexOptions.IgnoreCase);
                        if (regex.IsMatch(input))
                        {
                            // $0 in the replacement value is replaced by the input...need to escape it first to avoid getting into a recursive loop
                            // See https://stackoverflow.com/questions/41227351/trouble-with-0-in-regex-replace-c-sharp/41229868#41229868
                            input = regex.Replace(input, property.Value.Replace("$0", "$$0"));
                        }
                    }
                }
            } while (origInput != input && input.IndexOfAny(tokenChars) >= 0);

            return input;
        }
    }
}
