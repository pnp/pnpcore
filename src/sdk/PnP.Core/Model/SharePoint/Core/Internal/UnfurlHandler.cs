using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal static class UnfurlHandler
    {
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

            return new ApiCall($"sites/{context.Site.Id}/drives/{driveId}/items/{driveItemId}/thumbnails{thumbnailOptions}", ApiType.Graph);
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


    }
}
