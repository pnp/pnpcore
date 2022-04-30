namespace PnP.Core.Model.Security
{
    [GraphType]
    internal sealed class SharingLink : ISharingLink
    {
        public bool PreventsDownload { get; set; }
        public ShareType Type { get; set; }
        public ShareScope Scope { get; set; }
        public string WebHtml { get; set; }
        public string WebUrl { get; set; }
    }
}
