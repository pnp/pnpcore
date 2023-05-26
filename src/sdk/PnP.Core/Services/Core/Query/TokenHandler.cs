using PnP.Core.Model;
using System;
using System.Collections.Generic;
using System.Net;
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
                // Replace {IdAsPath}
                else if (match.Value.Equals("{IdAsPath}"))
                {
                    var model = pnpObject;

                    if (model.Metadata.ContainsKey(PnPConstants.MetaDataRestId))
                    {
                        // Encode the ID value to enable it to be used in methods using DecodedUrl input. Typically these methods end on Path
                        var idAsPathValue = WebUtility.UrlEncode(model.Metadata[PnPConstants.MetaDataRestId].Replace("'", "''").Replace("%20", " ")).Replace("+", "%20");
                        result = result.Replace("{IdAsPath}", idAsPathValue);
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
                        await ((IDataModelParent)pnpObject).EnsureParentObjectAsync().ConfigureAwait(false);
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
                        await ((IDataModelParent)pnpObject).EnsureParentObjectAsync().ConfigureAwait(false);
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
                        case "Url":
                            {
                                await context.Site.EnsurePropertiesAsync(p => p.Url).ConfigureAwait(false);
                                if (context.Site.HasValue(propertyToLoad))
                                {
                                    result = result.Replace(match.Value, context.Site.Url.ToString());
                                }
                                break;
                            }

                        case "HubSiteId":
                            {
                                await context.Site.EnsurePropertiesAsync(p => p.HubSiteId).ConfigureAwait(false);
                                if (context.Site.HasValue(propertyToLoad))
                                {
                                    result = result.Replace(match.Value, context.Site.HubSiteId.ToString());
                                }
                                break;
                            }
                    }
                }
                // Replace tokens coming from the Web object connected to the current PnPContext
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
                // Replace tokens coming from the List object connected to the current PnPContext
                else if (match.Value.StartsWith("{List.") && context != null)
                {
                    var propertyToLoad = match.Value.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("}", "");

                    switch (propertyToLoad)
                    {
                        case "Id":
                            {
                                // Try to see if the current object is a list
                                var list = pnpObject as Model.SharePoint.IList;

                                // If the object is a list item
                                if (list == null)
                                {
                                    if (pnpObject is Model.SharePoint.IListItem listItem)
                                    {
                                        // Get the parent list of the current list item
                                        list = GetParentDataModel(listItem as IMetadataExtensible) as Model.SharePoint.IList;

                                        // Option A: the file has the ListId property loaded
                                        if (list == null && GetParentDataModel(listItem as IMetadataExtensible) is Model.SharePoint.IFile file && file.IsPropertyAvailable(p => p.ListId))
                                        {
                                            result = result.Replace(match.Value, file.ListId.ToString());
                                            break;
                                        }

                                        // Option B: the IListItem has a property that indicates the used list
                                        if (list == null && (listItem as IMetadataExtensible).Metadata.ContainsKey(PnPConstants.MetaDataListId))
                                        {
                                            result = result.Replace(match.Value, (listItem as IMetadataExtensible).Metadata[PnPConstants.MetaDataListId]);
                                            break;
                                        }
                                    }
                                    else if (pnpObject is Model.SharePoint.IFile file)
                                    {
                                        listItem = GetParentDataModel(file as IMetadataExtensible) as Model.SharePoint.IListItem;
                                        list = GetParentDataModel(listItem as IMetadataExtensible) as Model.SharePoint.IList;
                                    }
                                    else if (pnpObject is Model.SharePoint.IFolder folder)
                                    {
                                        listItem = GetParentDataModel(folder as IMetadataExtensible) as Model.SharePoint.IListItem;
                                        list = GetParentDataModel(listItem as IMetadataExtensible) as Model.SharePoint.IList;
                                    }
                                    else if (pnpObject is Model.SharePoint.IFileVersion fileVersion)
                                    {
                                        if (fileVersion.Parent is Model.SharePoint.IFile)
                                        {
                                            var fileVersionfile = GetParentDataModel(fileVersion as IMetadataExtensible) as Model.SharePoint.IFile;
                                            listItem = GetParentDataModel(fileVersionfile as IMetadataExtensible) as Model.SharePoint.IListItem;
                                        }
                                        else if (fileVersion.Parent is Model.SharePoint.IListItemVersion)
                                        {
                                            var listItemVersion = GetParentDataModel(fileVersion as IMetadataExtensible) as Model.SharePoint.IListItemVersion;
                                            listItem = GetParentDataModel(listItemVersion as IMetadataExtensible) as Model.SharePoint.IListItem;
                                        }
                                        else
                                        {
                                            listItem = null;
                                        }

                                        list = GetParentDataModel(listItem as IMetadataExtensible) as Model.SharePoint.IList;
                                    }
                                    else if (pnpObject is Model.SharePoint.IListItemVersion listItemVersion)
                                    {
                                        listItem = GetParentDataModel(listItemVersion as IMetadataExtensible) as Model.SharePoint.IListItem;
                                        list = GetParentDataModel(listItem as IMetadataExtensible) as Model.SharePoint.IList;
                                    }
                                    else if (pnpObject is Model.SharePoint.IComment comment)
                                    {
                                        listItem = GetParentDataModel(comment as IMetadataExtensible) as Model.SharePoint.IListItem;

                                        if (listItem == null)
                                        {
                                            // comment was a reply to another comment
                                            var parentComment = GetParentDataModel(comment as IMetadataExtensible) as Model.SharePoint.IComment;
                                            listItem = GetParentDataModel(parentComment as IMetadataExtensible) as Model.SharePoint.IListItem;
                                        }

                                        // the IListItem has a property that indicates the used list
                                        if (listItem != null && (listItem as IMetadataExtensible).Metadata.ContainsKey(PnPConstants.MetaDataListId))
                                        {
                                            result = result.Replace(match.Value, (listItem as IMetadataExtensible).Metadata[PnPConstants.MetaDataListId]);
                                            break;
                                        }

                                        list = GetParentDataModel(listItem as IMetadataExtensible) as Model.SharePoint.IList;
                                    }
                                    else if (pnpObject is Model.SharePoint.IAttachment attachment)
                                    {
                                        listItem = GetParentDataModel(attachment as IMetadataExtensible) as Model.SharePoint.IListItem;
                                        list = GetParentDataModel(listItem as IMetadataExtensible) as Model.SharePoint.IList;
                                    }
                                }

                                // If we've got the list
                                if (list != null)
                                {
                                    // We retrieve the Id and we use it as the token value
                                    await list.EnsurePropertiesAsync(l => l.Id).ConfigureAwait(false);
                                    result = result.Replace(match.Value, list.Id.ToString());
                                }
                                break;
                            }
                    }
                }
                // Replace tokens coming from the List Item object connected to the current PnPContext
                else if (match.Value.StartsWith("{Item.") && context != null)
                {
                    var propertyToLoad = match.Value.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("}", "");

                    switch (propertyToLoad)
                    {
                        case "Id":
                            {
                                // Try to see if the current object is a list item
                                var listItem = pnpObject as Model.SharePoint.IListItem;

                                // If the object is a descendant of a list item
                                if (listItem == null)
                                {
                                    var model = pnpObject;
                                    if (model is Model.SharePoint.IFileVersion)
                                    {
                                        // Should return either an IListItemVersion or an IFile
                                        model = GetParentDataModel(model) as IMetadataExtensible;
                                    }

                                    if (model is Model.SharePoint.IFile || model is Model.SharePoint.IListItemVersion)
                                    {
                                        // Should return an IListItem
                                        listItem = GetParentDataModel(model) as Model.SharePoint.IListItem;
                                    }
                                }

                                // If we've got the list item
                                if (listItem != null)
                                {
                                    // We retrieve the Id and we use it as the token value
                                    await listItem.EnsurePropertiesAsync(l => l.Id).ConfigureAwait(false);
                                    result = result.Replace(match.Value, listItem.Id.ToString());
                                }
                                break;
                            }
                    }
                }
                // Replace TermSet.GraphId
                else if (match.Value.Equals("{TermSet.GraphId}"))
                {
                    bool replaced = false;
                    if (pnpObject is IMetadataExtensible withMetaData)
                    {
                        if (withMetaData.Metadata.ContainsKey(PnPConstants.MetaDataTermSetId))
                        {
                            result = result.Replace("{TermSet.GraphId}", withMetaData.Metadata[PnPConstants.MetaDataTermSetId]);
                            replaced = true;
                        }
                    }

                    if (!replaced)
                    {
                        IDataModelParent parent = GetParentDataModel(pnpObject);

                        if (parent is IMetadataExtensible p)
                        {
                            if (p.Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
                            {
                                result = result.Replace("{TermSet.GraphId}", p.Metadata[PnPConstants.MetaDataGraphId]);
                            }
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
                    match.Value.Equals("{List.Id}") ||
                    match.Value.Equals("{Item.Id}") ||
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