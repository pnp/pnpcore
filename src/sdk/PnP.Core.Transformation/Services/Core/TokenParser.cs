using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using PnP.Core.Transformation.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Resolves tokens by their actual representation
    /// </summary>
    public class TokenParser
    {
        private readonly ILogger<TokenParser> logger;
        private readonly IMemoryCache memoryCache;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Public constructor with dependency injection support
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="serviceProvider"></param>
        public TokenParser(
            ILogger<TokenParser> logger,
            IServiceProvider serviceProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceProvider = serviceProvider;
            this.memoryCache = this.serviceProvider.GetService<IMemoryCache>();
        }

        /// <summary>
        /// Replaces the tokens in the provided input string with their values working on the data source side of the transformation flow
        /// </summary>
        /// <param name="input">String with tokens</param>
        /// <param name="webPartProperties">Web part properties</param>
        /// <param name="globalProperties">Global context properties</param>
        /// <returns>A string with tokens replaced by actual values</returns>
        public string ReplaceSourceTokens(string input, Dictionary<string, string> webPartProperties, Dictionary<string, string> globalProperties = null)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            if (webPartProperties == null)
            {
                throw new ArgumentNullException(nameof(webPartProperties));
            }

            // Merge the web part properties and the global tokens, if any
            var properties = MergePropertiesAndGlobalTokens(webPartProperties, globalProperties);

            return ProcessStandardTokens(ref input, properties);
        }

        /// <summary>
        /// Replaces the tokens in the provided input string with their values
        /// </summary>
        /// <param name="context">Defines the target PnP Context</param>
        /// <param name="input">String with tokens</param>
        /// <param name="webPartProperties">Web part properties</param>
        /// <param name="globalProperties">Global context properties</param>
        /// <returns>A string with tokens replaced by actual values</returns>
        public string ReplaceTargetTokens(PnPContext context, string input, Dictionary<string, string> webPartProperties, Dictionary<string, string> globalProperties = null)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            if (webPartProperties == null)
            {
                throw new ArgumentNullException(nameof(webPartProperties));
            }

            // Merge the web part properties and the global tokens, if any
            var properties = MergePropertiesAndGlobalTokens(webPartProperties, globalProperties);

            input = ProcessStandardTokens(ref input, properties);

            // Retrieve the custom token processors
            var tokenDefinitions = this.serviceProvider.GetServices<ITokenDefinition>();

            // Manage the custom token definitions
            foreach (var property in properties)
            {
                if (!string.IsNullOrEmpty(property.Value) && property.Value.Contains("{"))
                {
                    foreach (var tokenDefinition in tokenDefinitions)
                    {
                        var regex = new Regex($"{{{tokenDefinition.Name}\\:(?<argument>[\\w\\|\\d\\-\\.\\:\\\\\\/]*)}}", RegexOptions.IgnoreCase);
                        if (regex.IsMatch(property.Value))
                        {
                            var match = regex.Match(property.Value);

                            // Get the matching "argument"
                            var argument = match.Groups["argument"]?.Value;

                            // Assign the token parser result to the input
                            var tokenValue = tokenDefinition.GetValue(context, argument);
                            if (tokenValue != null)
                            {
                                input = regex.Replace(input, tokenValue);
                            }
                        }
                    }
                }
            }

            return input;
        }

        private static Dictionary<string, string> MergePropertiesAndGlobalTokens(Dictionary<string, string> webPartProperties, Dictionary<string, string> globalProperties = null)
        {
            // Merge the web part properties and the global properties, if any
            var properties = webPartProperties.Merge(globalProperties);
            if (globalProperties != null && globalProperties.Count > 0)
            {
                foreach (var webPartPropertyKey in properties.Keys.ToList())
                {
                    if (globalProperties.ContainsKey(webPartPropertyKey))
                    {
                        properties[webPartPropertyKey] = globalProperties[webPartPropertyKey];
                    }
                }
            }

            return properties;
        }

        private static string ProcessStandardTokens(ref string input, Dictionary<string, string> properties)
        {
            var tokenChars = new[] { '{' };
            if (string.IsNullOrEmpty(input) || input.IndexOfAny(tokenChars) == -1)
            {
                return input;
            }

            // Manage any of the native tokens
            string origInput;
            do
            {
                origInput = input;
                foreach (var property in properties)
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
