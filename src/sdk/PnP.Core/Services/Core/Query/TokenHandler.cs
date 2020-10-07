using PnP.Core.Model;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// Handler class to help managing tokens replacement
    /// </summary>
    internal static class TokenHandler
    {
        internal static readonly Regex regex = new Regex("{(.*?)}", RegexOptions.Compiled);

        /// <summary>
        /// Method to resolve a set of tokens in a provided tokenized string
        /// </summary>
        /// <param name="tokenizedValue">A string with tokens</param>
        /// <param name="pnpObject">The domain model object to use as the target reference</param>
        /// <param name="context"><see cref="PnPContext"/> to get information from needed for token resolving</param>
        /// <returns>The string with tokens resolved</returns>
        public static async Task<string> ResolveTokensAsync(IMetadataExtensible pnpObject, string tokenizedValue, PnPContext context = null)
        {
            // Define the result variable
            string result = tokenizedValue;

            // Get the context aware version of the target pnpObject
            var contextAwareObject = pnpObject as IDataModelWithContext;

            // If we don't have an input context, let's try to use 
            // the one associated with the input entity, if any
            if (context == null && contextAwareObject != null)
            {
                context = contextAwareObject.PnPContext;
            }

            // Grab the tokens in this input (tokens are between curly braces)
            var matches = regex.Matches(tokenizedValue);

            // Iterate over the tokens and replace them
            foreach (Match match in matches)
            {
                // Replace {Id}
                if (match.Value.Equals("{Id}"))
                {
                    var model = pnpObject;

                    if (model.Metadata.ContainsKey(PnPConstants.MetaDataRestId))
                    {
                        result = result.Replace("{Id}", model.Metadata[PnPConstants.MetaDataRestId]);
                    }
                }
                // Replace {Parent.Id}
                else if (match.Value.Equals("{Parent.Id}"))
                {
                    // there's either a collection object inbetween (e.g. ListItem --> ListItemCollection --> List), so take the parent of the parent
                    // or
                    // the parent is model class itself (e.g. Web --> Site.RootWeb)
                    IDataModelParent parent = GetParentDataModel(pnpObject);

                    // Ensure the parent object
                    if (parent != null)
                    {
                        await ((IDataModelParent)pnpObject).EnsureParentObjectAsync().ConfigureAwait(true);
                    }

                    if (parent is IMetadataExtensible p)
                    {
                        if (p.Metadata.ContainsKey(PnPConstants.MetaDataRestId))
                        {
                            result = result.Replace("{Parent.Id}", p.Metadata[PnPConstants.MetaDataRestId]);
                        }
                    }
                }
                // Replace {GraphId}
                else if (match.Value.Equals("{GraphId}"))
                {
                    var model = pnpObject;

                    if (model.Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
                    {
                        result = result.Replace("{GraphId}", model.Metadata[PnPConstants.MetaDataGraphId]);
                    }
                }
                // Replace {Parent.GraphId}
                else if (match.Value.Equals("{Parent.GraphId}"))
                {
                    // there's either a collection object inbetween (e.g. TeamChannel --> TeamChannelCollection --> Team), so take the parent of the parent
                    // or
                    // the parent is model class itself (e.g. TeamChannel --> Team.PrimaryChannel)
                    IDataModelParent parent = GetParentDataModel(pnpObject);

                    // Ensure the parent object
                    if (parent != null)
                    {
                        await ((IDataModelParent)pnpObject).EnsureParentObjectAsync().ConfigureAwait(true);
                    }

                    if (parent is IMetadataExtensible p)
                    {
                        if (p.Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
                        {
                            result = result.Replace("{Parent.GraphId}", p.Metadata[PnPConstants.MetaDataGraphId]);
                        }
                    }
                }
                // Replace tokens coming from the Site object connected to the current PnPContext
                else if (match.Value.StartsWith("{Site.") && context != null)
                {
                    var propertyToLoad = match.Value.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("}", "");

                    switch (propertyToLoad)
                    {
                        case "GroupId":
                            {
                                await context.Site.EnsurePropertiesAsync(p => p.GroupId).ConfigureAwait(false);
                                if (context.Site.HasValue(propertyToLoad))
                                {
                                    result = result.Replace(match.Value, context.Site.GroupId.ToString());
                                }
                                break;
                            }
                        case "Id":
                            {
                                await context.Site.EnsurePropertiesAsync(p => p.Id).ConfigureAwait(false);
                                if (context.Site.HasValue(propertyToLoad))
                                {
                                    result = result.Replace(match.Value, context.Site.Id.ToString());
                                }
                                break;
                            }
                    }
                }
                // Replace tokens coming from the Site object connected to the current PnPContext
                else if (match.Value.StartsWith("{Web.") && context != null)
                {
                    var propertyToLoad = match.Value.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("}", "");

                    switch (propertyToLoad)
                    {
                        case "Id":
                            {
                                await context.Web.EnsurePropertiesAsync(p => p.Id).ConfigureAwait(false);
                                if (context.Web.HasValue(propertyToLoad))
                                {
                                    result = result.Replace(match.Value, context.Web.Id.ToString());
                                }
                                break;
                            }
                        case "GraphId":
                            {
                                var model = context.Web as IMetadataExtensible;

                                if (model.Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
                                {
                                    result = result.Replace("{Web.GraphId}", model.Metadata[PnPConstants.MetaDataGraphId]);
                                }
                                break;
                            }
                    }
                }
                // Replace {hostname}
                else if (match.Value.Equals("{hostname}"))
                {
                    result = result.Replace("{hostname}", context.Uri.DnsSafeHost);
                }
                // Replace {serverrelativepath}
                else if (match.Value.Equals("{serverrelativepath}"))
                {
                    result = result.Replace("{serverrelativepath}", context.Uri.PathAndQuery);
                }
            }

            return result;
        }

        internal static IDataModelParent GetParentDataModel(IMetadataExtensible pnpObject)
        {
            var parent = (pnpObject as IDataModelParent).Parent;

            if (parent is IManageableCollection)
            {
                // Parent is a collection, so jump one level up
                parent = (pnpObject as IDataModelParent).Parent.Parent;
            }

            return parent;
        }

        internal static List<string> UnresolvedTokens(string tokenizedValue)
        {
            // Grab the tokens in this input (tokens are between curly braces)
            var matches = regex.Matches(tokenizedValue);

            List<string> unresolvedTokens = new List<string>();

            // Iterate over the tokens and replace them
            foreach (Match match in matches)
            { 
                if (match.Value.Equals("{Id}") ||
                    match.Value.Equals("{Parent.Id}") ||
                    match.Value.Equals("{GraphId}") ||
                    match.Value.Equals("{Parent.GraphId}") ||
                    match.Value.Equals("{Site.GroupId}") ||
                    match.Value.Equals("{Site.Id}") ||
                    match.Value.Equals("{Web.Id}") ||
                    match.Value.Equals("{Web.GraphId}") ||
                    match.Value.Equals("{hostname") ||
                    match.Value.Equals("{serverrelativepath}"))
                {
                    unresolvedTokens.Add(match.Value);
                }
            }

            return unresolvedTokens;
        }

    }
}
