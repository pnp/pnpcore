namespace PnP.Core.Model.Security
{
    [GraphType]
    internal sealed class SharingLink : ISharingLink
    {
        public bool PreventsDownload { get; set; }
        public string Type { get; set; }
        public string Scope { get; set; }
        public string WebHtml { get; set; }
        public string WebUrl { get; set; }
    }
}
