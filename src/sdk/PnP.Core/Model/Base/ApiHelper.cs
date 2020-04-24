using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PnP.Core.Model
{
    /// <summary>
    /// Helper class to manage API calls URLs
    /// </summary>
    internal static class ApiHelper
    {
        internal static string ParseApiCall(TransientObject pnpObject, string apiCall, bool useGraph)
        {
            var metadataBasedObject = pnpObject as IMetadataExtensible;

            if (!useGraph && metadataBasedObject.Metadata.ContainsKey("uri"))
            {
                return metadataBasedObject.Metadata["uri"];
            }

            // No tokens, so nothing to do parse
            if (!apiCall.Contains("{"))
            {
                return apiCall;
            }

            // Parse api call to replace tokens
            apiCall = ParseApiRequest(metadataBasedObject, apiCall);

            return apiCall;
        }

        internal static string ParseApiRequest(IMetadataExtensible pnpObject, string input)
        {
            // Get the context aware version of the target pnpObject
            var contextAwareObject = pnpObject as IDataModelWithContext;

            // Grab the tokens in this input (tokens are between curly braces)
            var regex = new Regex("{(.*?)}", RegexOptions.Compiled);
            var matches = regex.Matches(input);

            // Iterate over the tokens and replace them
            foreach (Match match in matches)
            {
                // Replace {Id}
                if (match.Value.Equals("{Id}"))
                {
                    var model = pnpObject;

                    if (model.Metadata.ContainsKey(PnPConstants.MetaDataRestId))
                    {
                        input = input.Replace("{Id}", model.Metadata[PnPConstants.MetaDataRestId]);
                    }
                }

                // Replace {Parent.Id}
                if (match.Value.Equals("{Parent.Id}"))
                {
                    // there's always a collection object inbetween (e.g. ListItem --> ListItemCollection --> List), so take the parent of the parent
                    var model = (pnpObject as IDataModelParent).Parent.Parent as IMetadataExtensible;

                    if (model.Metadata.ContainsKey(PnPConstants.MetaDataRestId))
                    {
                        input = input.Replace("{Parent.Id}", model.Metadata[PnPConstants.MetaDataRestId]);
                    }
                }

                // Replace {GraphId}
                if (match.Value.Equals("{GraphId}"))
                {
                    var model = pnpObject;

                    if (model.Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
                    {
                        input = input.Replace("{GraphId}", model.Metadata[PnPConstants.MetaDataGraphId]);
                    }
                }

                // Replace {Parent.GraphId}
                if (match.Value.Equals("{Parent.GraphId}"))
                {
                    // there's always a collection object inbetween (e.g. ListItem --> ListItemCollection --> List), so take the parent of the parent
                    var parent = (pnpObject as IDataModelParent).Parent.Parent as IMetadataExtensible;

                    if (parent.Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
                    {
                        input = input.Replace("{Parent.GraphId}", parent.Metadata[PnPConstants.MetaDataGraphId]);
                    }
                }

                // Replace tokens coming from the Site object connected to the current PnPContext
                if (match.Value.StartsWith("{Site."))
                {
                    var propertyToLoad = match.Value.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("}", "");

                    switch (propertyToLoad)
                    {
                        case "GroupId":
                            {
                                contextAwareObject.PnPContext.Site.EnsurePropertiesAsync(p => p.GroupId).Wait();
                                if (contextAwareObject.PnPContext.Site.HasValue(propertyToLoad))
                                {
                                    input = input.Replace(match.Value, contextAwareObject.PnPContext.Site.GroupId.ToString());
                                }
                                break;
                            }
                        case "Id":
                            {
                                contextAwareObject.PnPContext.Site.EnsurePropertiesAsync(p => p.Id).Wait();
                                if (contextAwareObject.PnPContext.Site.HasValue(propertyToLoad))
                                {
                                    input = input.Replace(match.Value, contextAwareObject.PnPContext.Site.Id.ToString());
                                }
                                break;
                            }
                    }
                }

                // Replace tokens coming from the Site object connected to the current PnPContext
                if (match.Value.StartsWith("{Web."))
                {
                    var propertyToLoad = match.Value.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("}", "");

                    switch (propertyToLoad)
                    {
                        case "Id":
                            {
                                contextAwareObject.PnPContext.Web.EnsurePropertiesAsync(p => p.Id).Wait();
                                if (contextAwareObject.PnPContext.Web.HasValue(propertyToLoad))
                                {
                                    input = input.Replace(match.Value, contextAwareObject.PnPContext.Web.Id.ToString());
                                }
                                break;
                            }
                        case "GraphId":
                            {
                                var model = contextAwareObject.PnPContext.Web as IMetadataExtensible;

                                if (model.Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
                                {
                                    input = input.Replace("{Web.GraphId}", model.Metadata[PnPConstants.MetaDataGraphId]);
                                }
                                break;
                            }
                    }
                }
            }

            return input;
        }
    }
}
