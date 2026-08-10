using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Result of a CAML query executed via CSOM, see LoadItemsByCamlQueryViaCsomAsync on <see cref="IList"/>
    /// </summary>
    public interface ICamlQueryCsomResult
    {
        /// <summary>
        /// The list items returned by this query. The items are also merged into the list's Items collection.
        /// </summary>
        IReadOnlyList<IListItem> Items { get; }

        /// <summary>
        /// Paging information to get the next page of data: set this as <see cref="CamlQueryOptions.PagingInfo"/>
        /// on the next query. Null when there are no more pages.
        /// </summary>
        string PagingInfo { get; }
    }
}
