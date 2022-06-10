using PnP.Core.Services;
using PnP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal static class UnfurlHandler
    {
        internal static async Task<IUnfurledResource> UnfurlAsync(PnPContext context, string link, UnfurlOptions options = null)
        {
            UnfurledResource resource = new UnfurledResource
            {
                LinkType = UnfurlLinkType.Unknown
            };

            // first encode the passed link
            var encodedLink = DriveHelper.EncodeSharingUrl(link);

            // Build the needed Graph calls and process them as a single batch
            var batch = context.NewBatch();
            foreach (var apiCall in BuildUnfurlApiCalls(resource, encodedLink, options))
            {
                await (context.Web as Web).RawRequestBatchAsync(batch, apiCall, HttpMethod.Get).ConfigureAwait(false);
            }

            // Execute the batch and process the responses
            await context.ExecuteAsync(batch, false).ConfigureAwait(false);
            
            return resource;
        }

        private static List<ApiCall> BuildUnfurlApiCalls(UnfurledResource resource, string encodedLink, UnfurlOptions options)
        {            
            var response = new List<ApiCall>
            {
                // Request 1: let's try to get list information
                new ApiCall($"shares/{encodedLink}/list?$select=sharepointIds,lastModifiedDateTime,webUrl,list,id,displayname,lastModifiedBy", ApiType.Graph)
                {
                    RawSingleResult = resource,
                    RawResultsHandler = (json, apiCall) =>
                    {
                        if (!string.IsNullOrEmpty(json))
                        {
                            DeserializeGraphList((UnfurledResource)apiCall.RawSingleResult, json);
                        }
                    }
                },

                // Request 2: let's try to get ListItem information
                new ApiCall($"shares/{encodedLink}/listitem", ApiType.Graph)
                {
                    RawSingleResult = resource,
                    RawResultsHandler = (json, apiCall) =>
                    {
                        if (!string.IsNullOrEmpty(json))
                        {
                            DeserializeGraphListItem((UnfurledResource)apiCall.RawSingleResult, json);
                        }
                    }
                },

                // Request 3: let's try to get DriveItem information
                new ApiCall($"shares/{encodedLink}/driveitem", ApiType.Graph)
                {
                    RawSingleResult = resource,
                    RawResultsHandler = (json, apiCall) =>
                    {
                        if (!string.IsNullOrEmpty(json))
                        {
                            DeserializeGraphDriveItem((UnfurledResource)apiCall.RawSingleResult, json);
                        }
                    }
                },
            };

            // Request 4: let's try to get thumbnails
            string thumbnailOptions = "";
            if (options != null && options.ThumbnailOptions != null)
            {
                thumbnailOptions = BuildThumbnailOptions(options.ThumbnailOptions);
            }
            response.Add(
                new ApiCall($"shares/{encodedLink}/driveitem/thumbnails{thumbnailOptions}", ApiType.Graph)
                {
                    RawSingleResult = resource,
                    RawResultsHandler = (json, apiCall) =>
                    {
                        if (((UnfurledResource)apiCall.RawSingleResult).Thumbnails == null)
                        {
                            ((UnfurledResource)apiCall.RawSingleResult).Thumbnails = new List<IThumbnail>();
                        }

                        if (!string.IsNullOrEmpty(json))
                        {
                            DeserializeRetrievedThumbnails(((UnfurledResource)apiCall.RawSingleResult).Thumbnails, json);
                        }
                    },
                });

            return response;
        }


        internal static async Task<List<IThumbnail>> GetThumbnailsAsync(PnPContext context, string driveId, string driveItemId, ThumbnailOptions options = null)
        {
            List<IThumbnail> thumbnails = new List<IThumbnail>();

            if (string.IsNullOrEmpty(driveId))
            {
                throw new ArgumentNullException(nameof(driveId));
            }

            if (string.IsNullOrEmpty(driveItemId))
            {
                throw new ArgumentNullException(nameof(driveItemId));
            }

            ApiCall apiCall = BuildThumbnailApiCall(context, driveId, driveItemId, options);
            var response = await (context.Web as Web).RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Json))
            {
                DeserializeRetrievedThumbnails(thumbnails, response.Json);
            }

            return thumbnails;
        }

        public static async Task<IEnumerableBatchResult<IThumbnail>> GetThumbnailsBatchAsync(Batch batch, PnPContext context, string driveId, string driveItemId, ThumbnailOptions options = null)
        {
            if (string.IsNullOrEmpty(driveId))
            {
                throw new ArgumentNullException(nameof(driveId));
            }

            if (string.IsNullOrEmpty(driveItemId))
            {
                throw new ArgumentNullException(nameof(driveItemId));
            }

            ApiCall apiCall = BuildThumbnailApiCall(context, driveId, driveItemId, options);
            apiCall.RawEnumerableResult = new List<IThumbnail>();
            apiCall.RawResultsHandler = (json, apiCall) =>
            {
                DeserializeRetrievedThumbnails((List<IThumbnail>)apiCall.RawEnumerableResult, json);
            };

            var batchRequest = await (context.Web as Web).RawRequestBatchAsync(batch, apiCall, HttpMethod.Get).ConfigureAwait(false);

            return new BatchEnumerableBatchResult<IThumbnail>(batch, batchRequest.Id, (IReadOnlyList<IThumbnail>)apiCall.RawEnumerableResult);
        }


        private static ApiCall BuildThumbnailApiCall(PnPContext context, string driveId, string driveItemId, ThumbnailOptions options)
        {
            string thumbnailOptions = BuildThumbnailOptions(options);

            return new ApiCall($"sites/{context.Site.Id}/drives/{driveId}/items/{driveItemId}/thumbnails{thumbnailOptions}", ApiType.Graph);
        }

        private static string BuildThumbnailOptions(ThumbnailOptions options)
        {
            string thumbnailOptions = "";

            if (options != null)
            {
                List<string> sizes = new List<string>();
                if (options.StandardSizes != null && options.StandardSizes.Count > 0)
                {
                    foreach (var size in options.StandardSizes)
                    {
                        sizes.Add($"{size.ToString().ToLower()}");
                    }
                }

                if (options.CustomSizes != null && options.CustomSizes.Count > 0)
                {
                    foreach (var size in options.CustomSizes)
                    {
                        string crop = size.Cropped ? "_crop" : "";
                        sizes.Add($"c{size.Width}x{size.Height}{crop}");
                    }
                }

                thumbnailOptions = $"?$select={string.Join(",", sizes)}";
            }

            return thumbnailOptions;
        }

        private static void DeserializeRetrievedThumbnails(List<IThumbnail> thumbnails, string response)
        {
            var json = JsonSerializer.Deserialize<JsonElement>(response);

            if (json.TryGetProperty("value", out JsonElement thumbnailSet))
            {
                foreach (var set in thumbnailSet.EnumerateArray())
                {
                    Thumbnail thumbnail = null;
                    string lastUsedSet = null;
                    foreach (var node in set.EnumerateObject())
                    {
                        if (node.Name != "id")
                        {
                            if (thumbnail == null)
                            {
                                thumbnail = new Thumbnail();
                            }

                            thumbnail.Size = node.Name.Replace("_x005f_", "_").Replace("_x0020_", " ");

                            if (node.Value.TryGetProperty("url", out JsonElement thumbnailUrl) && Uri.TryCreate(thumbnailUrl.GetString(), UriKind.RelativeOrAbsolute, out Uri thumbnailUri))
                            {
                                thumbnail.Url = thumbnailUri;
                            }

                            if (node.Value.TryGetProperty("height", out JsonElement thumbnailHeight))
                            {
                                thumbnail.Height = thumbnailHeight.GetInt32();
                            }

                            if (node.Value.TryGetProperty("width", out JsonElement thumbnailWidth))
                            {
                                thumbnail.Width = thumbnailWidth.GetInt32();
                            }

                            thumbnails.Add(thumbnail);
                            thumbnail = new Thumbnail
                            {
                                SetId = lastUsedSet
                            };
                        }
                        else
                        {
                            if (set.TryGetProperty("id", out JsonElement setIdString))
                            {
                                thumbnail = new Thumbnail
                                {
                                    SetId = setIdString.GetString()
                                };

                                lastUsedSet = thumbnail.SetId;
                            }
                        }
                    }
                }
            }
        }

        private static void DeserializeGraphList(UnfurledResource resource, string response)
        {
            var json = JsonSerializer.Deserialize<JsonElement>(response);

            if (json.TryGetProperty("list", out JsonElement list) && list.TryGetProperty("template", out JsonElement template))
            {
                if (template.GetString() == "documentLibrary")
                {
                    resource.LinkType = UnfurlLinkType.Library;
                }
                else if (template.GetString() == "genericList")
                {
                    resource.LinkType = UnfurlLinkType.List;
                }
                else if (template.GetString() == "webPageLibrary")
                {
                    resource.LinkType = UnfurlLinkType.SitePagesLibrary;
                }
            }

            if (json.TryGetProperty("sharepointIds", out JsonElement sharepointIds))
            {

                if (sharepointIds.TryGetProperty("siteId", out JsonElement siteId))
                {
                    resource.SiteId = siteId.GetGuid();
                }

                if (sharepointIds.TryGetProperty("webId", out JsonElement webId))
                {
                    resource.WebId = webId.GetGuid();
                }

                if (sharepointIds.TryGetProperty("siteUrl", out JsonElement siteUrl))
                {
                    resource.WebUrl = new Uri(siteUrl.ToString());
                }
            }

            if (json.TryGetProperty("id", out JsonElement id))
            {
                resource.ListId = id.GetGuid();
            }

            if (json.TryGetProperty("displayName", out JsonElement displayName))
            {
                resource.ListDisplayName = displayName.GetString();
                resource.Name = displayName.GetString();
            }

            if (json.TryGetProperty("webUrl", out JsonElement webUrl))
            {
                resource.Resource = new Uri(webUrl.GetString());
                resource.ListUrl = new Uri(webUrl.GetString());
            }

            if (json.TryGetProperty("lastModifiedDateTime", out JsonElement lastModifiedDateTime))
            {
                resource.LastModified = lastModifiedDateTime.GetDateTime();
            }

            if (json.TryGetProperty("lastModifiedBy", out JsonElement lastModifiedBy) && 
                lastModifiedBy.TryGetProperty("user", out JsonElement user) && 
                user.TryGetProperty("displayName", out JsonElement displayNameModified))
            {
                resource.LastModifiedBy = displayNameModified.GetString();
            }

        }

        private static void DeserializeGraphListItem(UnfurledResource resource, string response)
        {
            var json = JsonSerializer.Deserialize<JsonElement>(response);

            if (json.TryGetProperty("lastModifiedDateTime", out JsonElement lastModifiedDateTime))
            {
                resource.LastModified = lastModifiedDateTime.GetDateTime();
            }

            if (json.TryGetProperty("id", out JsonElement id))
            {
                if (int.TryParse(id.GetString(), out int listItemId))
                {
                    resource.ListItemId = listItemId;
                }
            }

            if (json.TryGetProperty("lastModifiedBy", out JsonElement lastModifiedBy) &&
                lastModifiedBy.TryGetProperty("user", out JsonElement user) &&
                user.TryGetProperty("displayName", out JsonElement displayNameModified))
            {
                resource.LastModifiedBy = displayNameModified.GetString();
            }

            if (resource.LinkType == UnfurlLinkType.SitePagesLibrary)
            {
                if (json.TryGetProperty("fields", out JsonElement fields) && fields.TryGetProperty("FileLeafRef", out JsonElement fileLeafRef))
                {
                    if (!string.IsNullOrEmpty(fileLeafRef.GetString()))
                    {
                        resource.LinkType = UnfurlLinkType.SitePage;
                    }
                }
            }
            else
            {
                resource.LinkType = UnfurlLinkType.ListItem;
            }

            if (resource.LinkType != UnfurlLinkType.ListItem && json.TryGetProperty("webUrl", out JsonElement webUrl))
            {
                resource.Resource = new Uri(webUrl.GetString());
            }

            if (json.TryGetProperty("fields", out JsonElement fields1))
            {
                if (fields1.TryGetProperty("Title", out JsonElement title))
                {
                    resource.Name = title.GetString();
                }

                if (fields1.TryGetProperty("FileSizeDisplay", out JsonElement fileSizeDisplay))
                {
                    if (long.TryParse(fileSizeDisplay.GetString(), out long fileSize))
                    {
                        resource.Size = fileSize;
                    }
                }
            }
        }

        private static void DeserializeGraphDriveItem(UnfurledResource resource, string response)
        {
            var json = JsonSerializer.Deserialize<JsonElement>(response);
            
            if (json.TryGetProperty("webUrl", out JsonElement webUrl))
            {
                resource.Resource = new Uri(webUrl.GetString());
            }

            if (!json.TryGetProperty("root", out JsonElement root))
            {
                if (json.TryGetProperty("name", out JsonElement name))
                {
                    resource.Name = name.GetString();
                }

                if (json.TryGetProperty("parentReference", out JsonElement parentReference))
                {
                    if (parentReference.TryGetProperty("driveId", out JsonElement driveId))
                    {
                        resource.FileDriveId = driveId.GetString();
                    }

                    if (parentReference.TryGetProperty("id", out JsonElement driveItemId))
                    {
                        resource.FileDriveItemId = driveItemId.GetString();
                    }

                    if (!string.IsNullOrEmpty(resource.FileDriveItemId))
                    {
                        resource.FileUniqueId = DriveHelper.DecodeDriveItemId(resource.FileDriveItemId);
                    }
                }

                resource.LinkType = UnfurlLinkType.File;
            }
        }

    }
}
