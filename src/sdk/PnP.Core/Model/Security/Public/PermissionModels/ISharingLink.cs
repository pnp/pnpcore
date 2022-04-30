namespace PnP.Core.Model.Security
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(SharingLink))]
    public interface ISharingLink
    {
        /// <summary>
        /// 
        /// </summary>
        public bool PreventsDownload { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Scope { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string WebHtml { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string WebUrl { get; set; }
    }
}
