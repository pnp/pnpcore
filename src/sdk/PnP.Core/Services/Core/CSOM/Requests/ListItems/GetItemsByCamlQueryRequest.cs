using PnP.Core.Model.SharePoint;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Services.Core.CSOM.Requests.ListItems
{
    /// <summary>
    /// Executes a CAML query via the CSOM List.GetItems(CamlQuery) method, which unlike the REST GetItems
    /// endpoint also returns fields projected from joined lists (CAML Joins/ProjectedFields)
    /// </summary>
    internal class GetItemsByCamlQueryRequest : IRequest<GetItemsByCamlQueryResponse>
    {
        internal GetItemsByCamlQueryRequest(string siteId, string webId, string listId, CamlQueryOptions queryOptions)
        {
            SiteId = siteId;
            WebId = webId;
            ListId = listId;
            QueryOptions = queryOptions;
        }

        internal string SiteId { get; }

        internal string WebId { get; }

        internal string ListId { get; }

        internal CamlQueryOptions QueryOptions { get; }

        internal int QueryActionId { get; private set; }

        public GetItemsByCamlQueryResponse Result { get; } = new GetItemsByCamlQueryResponse();

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            Identity listIdentity = new Identity
            {
                Id = idProvider.GetActionId(),
                Name = $"121a659f-e03e-2000-4281-1212829d67dd|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{SiteId}:web:{WebId}:list:{ListId}"
            };

            ObjectPathMethod getItemsMethod = new ObjectPathMethod
            {
                Id = idProvider.GetActionId(),
                ParentId = listIdentity.Id,
                Name = "GetItems",
                Parameters = new MethodParameter
                {
                    Properties = new List<Parameter>
                    {
                        new CamlQueryParameter(QueryOptions)
                    }
                }
            };

            List<ActionObjectPath> result = new List<ActionObjectPath>
            {
                new ActionObjectPath
                {
                    ObjectPath = listIdentity,
                    Action = new BaseAction
                    {
                        Id = idProvider.GetActionId(),
                        ObjectPathId = getItemsMethod.Id.ToString()
                    }
                }
            };

            QueryActionId = idProvider.GetActionId();

            result.Add(new ActionObjectPath
            {
                ObjectPath = getItemsMethod,
                Action = new QueryAction.QueryAction
                {
                    Id = QueryActionId,
                    ObjectPathId = getItemsMethod.Id.ToString(),
                    // SelectAllProperties on the item collection also returns the ListItemCollectionPosition
                    // needed to request the next page
                    SelectQuery = new SelectQuery
                    {
                        SelectAllProperties = true,
                        Properties = new List<Property>()
                    },
                    ChildItemQuery = new ChildItemQuery
                    {
                        SelectAllProperties = true
                    }
                }
            });

            return result;
        }

        public void ProcessResponse(string response)
        {
            List<JsonElement> results = JsonSerializer.Deserialize<List<JsonElement>>(response, PnPConstants.JsonSerializer_SPGuidConverter_DateTimeConverter);

            if (results == null)
            {
                return;
            }

            int idIndex = results.FindIndex(r => CSOMResponseHelper.CompareIdElement(r, QueryActionId));

            // The element following the query action id holds the SP.ListItemCollection
            if (idIndex < 0 || idIndex + 1 >= results.Count)
            {
                return;
            }

            JsonElement itemCollection = results[idIndex + 1];

            if (itemCollection.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            if (itemCollection.TryGetProperty("_Child_Items_", out JsonElement childItems) && childItems.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in childItems.EnumerateArray())
                {
                    Result.Items.Add(item);
                }
            }

            if (itemCollection.TryGetProperty("ListItemCollectionPosition", out JsonElement position) &&
                position.ValueKind == JsonValueKind.Object &&
                position.TryGetProperty("PagingInfo", out JsonElement pagingInfo) &&
                pagingInfo.ValueKind == JsonValueKind.String)
            {
                Result.PagingInfo = pagingInfo.GetString();
            }
        }
    }
}
