namespace PnP.Core.Model.SharePoint
{
    public interface IAlternateUICulture : IDataModel<IAlternateUICulture>
    {
        /// <summary>
        /// The Locale ID of a AlternateUICulture
        /// </summary>
        public int LCID { get; set; }
    }
}
