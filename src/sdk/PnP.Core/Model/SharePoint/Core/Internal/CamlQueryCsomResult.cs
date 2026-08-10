using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class CamlQueryCsomResult : ICamlQueryCsomResult
    {
        internal CamlQueryCsomResult(IReadOnlyList<IListItem> items, string pagingInfo)
        {
            Items = items;
            PagingInfo = pagingInfo;
        }

        public IReadOnlyList<IListItem> Items { get; }

        public string PagingInfo { get; }
    }
}
