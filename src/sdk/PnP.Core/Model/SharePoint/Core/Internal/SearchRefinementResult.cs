namespace PnP.Core.Model.SharePoint
{
    internal sealed class SearchRefinementResult : ISearchRefinementResult
    {
        public long Count { get; set; }

        public string Name { get; set; }

        public string Token { get; set; }

        public string Value { get; set; }
    }
}
