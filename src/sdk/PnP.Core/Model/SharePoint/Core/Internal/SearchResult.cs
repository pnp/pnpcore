using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class SearchResult : ISearchResult
    {
        public int ElapsedTime { get; set; }

        public long TotalRows { get; set; }

        public long TotalRowsIncludingDuplicates { get; set; }

        public List<Dictionary<string, object>> Rows { get; } = new List<Dictionary<string, object>>();

        public Dictionary<string, List<ISearchRefinementResult>> Refinements { get; } = new Dictionary<string, List<ISearchRefinementResult>>();
    }
}
