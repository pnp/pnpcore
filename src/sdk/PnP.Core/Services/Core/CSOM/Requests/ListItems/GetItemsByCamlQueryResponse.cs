using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Services.Core.CSOM.Requests.ListItems
{
    /// <summary>
    /// Result of a CSOM List.GetItems(CamlQuery) call
    /// </summary>
    internal class GetItemsByCamlQueryResponse
    {
        /// <summary>
        /// The returned SP.ListItem json objects
        /// </summary>
        internal List<JsonElement> Items { get; } = new List<JsonElement>();

        /// <summary>
        /// Paging information to feed into the next call to get the next page, null when there are no more pages
        /// </summary>
        internal string PagingInfo { get; set; }
    }
}
