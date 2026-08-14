using PnP.Core.Provisioning.Model;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.Publishing
{
    /// <summary>
    /// Reads the site collection's image renditions.
    /// </summary>
    internal sealed class GetImageRenditionsRequest : IRequest<List<ImageRenditionInfo>>
    {
        public List<ImageRenditionInfo> Result { get; private set; } = new List<ImageRenditionInfo>();

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        internal int RenditionsQueryId { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var result = new List<ActionObjectPath>();

            var getRenditions = new StaticMethodPath
            {
                Id = idProvider.GetActionId(),
                TypeId = CsomTypeIds.SiteImageRenditions,
                Name = "GetRenditions",
                Parameters = new MethodParameter { Properties = new List<Parameter>() }
            };

            result.Add(new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = getRenditions.Id.ToString()
                },
                ObjectPath = getRenditions
            });

            RenditionsQueryId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                Action = new QueryAction
                {
                    Id = RenditionsQueryId,
                    ObjectPathId = getRenditions.Id.ToString(),
                    SelectQuery = new SelectQuery
                    {
                        SelectAllProperties = false,
                        Properties = new List<Property>()
                    },
                    ChildItemQuery = new ChildItemQuery
                    {
                        SelectAllProperties = true,
                        Properties = new List<Property>()
                    }
                }
            });

            return result;
        }

        public void ProcessResponse(string response)
        {
            JsonElement renditions = ResponseHelper.ProcessResponse<JsonElement>(response, RenditionsQueryId);

            if (renditions.ValueKind != JsonValueKind.Object
                || !renditions.TryGetProperty("_Child_Items_", out JsonElement items)
                || items.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            var parsed = new List<ImageRenditionInfo>();
            foreach (JsonElement item in items.EnumerateArray())
            {
                parsed.Add(new ImageRenditionInfo
                {
                    Id = item.TryGetProperty("Id", out JsonElement id) && id.ValueKind == JsonValueKind.Number ? id.GetInt32() : 0,
                    Name = item.TryGetProperty("Name", out JsonElement name) ? name.GetString() : null,
                    Width = item.TryGetProperty("Width", out JsonElement width) && width.ValueKind == JsonValueKind.Number ? width.GetInt32() : 0,
                    Height = item.TryGetProperty("Height", out JsonElement height) && height.ValueKind == JsonValueKind.Number ? height.GetInt32() : 0,
                    Version = item.TryGetProperty("Version", out JsonElement version) && version.ValueKind == JsonValueKind.Number ? version.GetInt32() : 0
                });
            }

            Result = parsed;
        }
    }
}
