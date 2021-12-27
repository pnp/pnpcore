namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Defines the chrome (header/footer) options of a web
    /// </summary>
    public interface IChromeOptions
    {
        /// <summary>
        /// Site header chrome configuration
        /// </summary>
        IHeaderOptions Header { get; }

        /// <summary>
        /// Site navigation chrome options
        /// </summary>
        INavigationOptions Navigation { get; }

        /// <summary>
        /// Site footer chrome configuration
        /// </summary>
        IFooterOptions Footer { get; }

    }
}
