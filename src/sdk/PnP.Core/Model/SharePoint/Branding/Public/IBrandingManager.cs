using PnP.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Enables branding changes for the site (the options that appear under "change the look" under the gear icon in SharePoint)
    /// </summary>
    public interface IBrandingManager
    {
        #region Enumerating themes and setting them on the site

        /// <summary>
        /// Lists the available themes for this web
        /// </summary>
        /// <returns>List of <see cref="ITheme"/>instances</returns>
        Task<List<ITheme>> GetAvailableThemesAsync();

        /// <summary>
        /// Lists the available themes for this web
        /// </summary>
        /// <returns>List of <see cref="ITheme"/>instances</returns>
        List<ITheme> GetAvailableThemes();

        /// <summary>
        /// Lists the available themes for this web
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <returns>List of <see cref="ITheme"/>instances</returns>
        Task<IEnumerableBatchResult<ITheme>> GetAvailableThemesBatchAsync(Batch batch);

        /// <summary>
        /// Lists the available themes for this web
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <returns>List of <see cref="ITheme"/>instances</returns>
        IEnumerableBatchResult<ITheme> GetAvailableThemesBatch(Batch batch);

        /// <summary>
        /// Lists the available themes for this web
        /// </summary>
        /// <returns>List of <see cref="ITheme"/>instances</returns>
        Task<IEnumerableBatchResult<ITheme>> GetAvailableThemesBatchAsync();

        /// <summary>
        /// Lists the available themes for this web
        /// </summary>
        /// <returns>List of <see cref="ITheme"/>instances</returns>
        IEnumerableBatchResult<ITheme> GetAvailableThemesBatch();

        /// <summary>
        /// Sets a an out of the box theme
        /// </summary>
        /// <param name="theme">Theme to apply</param>
        /// <returns></returns>
        Task SetThemeAsync(SharePointTheme theme);

        /// <summary>
        /// Sets a an out of the box theme
        /// </summary>
        /// <param name="theme">Theme to apply</param>
        /// <returns></returns>
        void SetTheme(SharePointTheme theme);

        /// <summary>
        /// Sets a an out of the box theme
        /// </summary>
        /// <param name="theme">Theme to apply</param>
        /// <returns></returns>
        Task SetThemeBatchAsync(SharePointTheme theme);

        /// <summary>
        /// Sets a an out of the box theme
        /// </summary>
        /// <param name="theme">Theme to apply</param>
        /// <returns></returns>
        void SetThemeBatch(SharePointTheme theme);

        /// <summary>
        /// Sets a an out of the box theme
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="theme">Theme to apply</param>
        /// <returns></returns>
        Task SetThemeBatchAsync(Batch batch, SharePointTheme theme);

        /// <summary>
        /// Sets a an out of the box theme
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="theme">Theme to apply</param>
        /// <returns></returns>
        void SetThemeBatch(Batch batch, SharePointTheme theme);

        /// <summary>
        /// Sets a custom theme
        /// </summary>
        /// <param name="theme">Theme to apply</param>
        /// <returns></returns>
        Task SetThemeAsync(ITheme theme);

        /// <summary>
        /// Sets a custom theme
        /// </summary>
        /// <param name="theme">Theme to apply</param>
        /// <returns></returns>
        void SetTheme(ITheme theme);

        /// <summary>
        /// Sets a custom theme
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="theme">Theme to apply</param>
        /// <returns></returns>
        Task SetThemeBatchAsync(Batch batch, ITheme theme);

        /// <summary>
        /// Sets a custom theme
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="theme">Theme to apply</param>
        /// <returns></returns>
        void SetThemeBatch(Batch batch, ITheme theme);

        /// <summary>
        /// Sets a custom theme
        /// </summary>
        /// <param name="theme">Theme to apply</param>
        /// <returns></returns>
        Task SetThemeBatchAsync(ITheme theme);

        /// <summary>
        /// Sets a custom theme
        /// </summary>
        /// <param name="theme">Theme to apply</param>
        /// <returns></returns>
        void SetThemeBatch(ITheme theme);

        #endregion

        #region Site chrome 

        /// <summary>
        /// Gets the site's chrome (header/footer/navigation) options
        /// </summary>
        /// <returns>Site's <see cref="IChromeOptions"/></returns>
        Task<IChromeOptions> GetChromeOptionsAsync();

        /// <summary>
        /// Gets the site's chrome (header/footer/navigation) options
        /// </summary>
        /// <returns>Site's <see cref="IChromeOptions"/></returns>
        IChromeOptions GetChromeOptions();

        /// <summary>
        /// Gets the site's chrome (header/footer/navigation) options
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <returns>Site's <see cref="IChromeOptions"/></returns>
        Task<IBatchSingleResult<IChromeOptions>> GetChromeOptionsBatchAsync(Batch batch);

        /// <summary>
        /// Gets the site's chrome (header/footer/navigation) options
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <returns>Site's <see cref="IChromeOptions"/></returns>
        IBatchSingleResult<IChromeOptions> GetChromeOptionsBatch(Batch batch);

        /// <summary>
        /// Gets the site's chrome (header/footer/navigation) options
        /// </summary>
        /// <returns>Site's <see cref="IChromeOptions"/></returns>
        Task<IBatchSingleResult<IChromeOptions>> GetChromeOptionsBatchAsync();

        /// <summary>
        /// Gets the site's chrome (header/footer/navigation) options
        /// </summary>
        /// <returns>Site's <see cref="IChromeOptions"/></returns>
        IBatchSingleResult<IChromeOptions> GetChromeOptionsBatch();

        /// <summary>
        /// Sets the site's chrome (header/footer/navigation) options
        /// </summary>
        /// <param name="chromeOptions">Site chrome options to apply</param>
        /// <returns></returns>
        Task SetChromeOptionsAsync(IChromeOptions chromeOptions);

        /// <summary>
        /// Sets the site's chrome (header/footer/navigation) options
        /// </summary>
        /// <param name="chromeOptions">Site chrome options to apply</param>
        /// <returns></returns>
        void SetChromeOptions(IChromeOptions chromeOptions);

        /// <summary>
        /// Sets the site's chrome (header/footer/navigation) options
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="chromeOptions">Site chrome options to apply</param>
        /// <returns></returns>
        Task SetChromeOptionsBatchAsync(Batch batch, IChromeOptions chromeOptions);

        /// <summary>
        /// Sets the site's chrome (header/footer/navigation) options
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="chromeOptions">Site chrome options to apply</param>
        /// <returns></returns>
        void SetChromeOptionsBatch(Batch batch, IChromeOptions chromeOptions);

        /// <summary>
        /// Sets the site's chrome (header/footer/navigation) options
        /// </summary>
        /// <param name="chromeOptions">Site chrome options to apply</param>
        /// <returns></returns>
        Task SetChromeOptionsBatchAsync(IChromeOptions chromeOptions);

        /// <summary>
        /// Sets the site's chrome (header/footer/navigation) options
        /// </summary>
        /// <param name="chromeOptions">Site chrome options to apply</param>
        /// <returns></returns>
        void SetChromeOptionsBatch(IChromeOptions chromeOptions);
        #endregion

        #region OutOfBoxFontPackages

        /// <summary>
        /// Gets the out of the box fonts available
        /// </summary>
        /// <returns>Site's <see cref="IFontPackage"/></returns>
        Task<List<IFontPackage>> GetOutOfBoxFontPackagesAsync();

        /// <summary>
        /// Gets the out of the box fonts available
        /// </summary>
        /// <returns>Site's <see cref="IFontPackage"/></returns>
        List<IFontPackage> GetOutOfBoxFontPackages();

        /// <summary>
        /// Gets the out of the box fonts available
        /// </summary>
        /// <param name="batch"></param>
        /// <returns>Site's <see cref="IFontPackage"/></returns>
        Task<IBatchSingleResult<List<IFontPackage>>> GetOutOfBoxFontPackagesBatchAsync(Batch batch);

        /// <summary>
        /// Gets the out of the box fonts available
        /// </summary>
        /// <returns>Site's <see cref="System.Collections.Generic.List{IFontPackage}"/></returns>
        IBatchSingleResult<List<IFontPackage>> GetOutOfBoxFontPackagesBatch(Batch batch);

        /// <summary>
        /// Sets the out of the box font with the given id on the Site
        /// </summary>
        /// <param name="fontId"></param>
        void SetOutOfBoxFontPackage(string fontId);

        /// <summary>
        /// Sets the out of the box font with the given id on the Site
        /// </summary>
        /// <param name="fontId"></param>
        Task SetOutOfBoxFontPackageAsync(string fontId);

        /// <summary>
        /// Sets the out of the box font with the given id on the Site
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="fontId"></param>
        void SetOutOfBoxFontPackageBatch(Batch batch, string fontId);

        /// <summary>
        /// Sets the out of the box font with the given id on the Site
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="fontId"></param>
        Task SetOutOfBoxFontPackageBatchAsync(Batch batch, string fontId);

        #endregion OutOfBoxFontPackages

        #region Branding Center FontPackages

        /// <summary>
        /// Gets the fonts available in branding center
        /// </summary>
        /// <returns>Site's <see cref="System.Collections.Generic.List{IFontPackage}"/></returns>
        Task<List<IFontPackage>> GetFontPackagesAsync();

        /// <summary>
        /// Gets the fonts available in branding center
        /// </summary>
        /// <returns>Site's <see cref="System.Collections.Generic.List{IFontPackage}"/></returns>
        List<IFontPackage> GetFontPackages();

        /// <summary>
        /// Gets the fonts available in branding center
        /// </summary>
        /// <returns>Site's <see cref="System.Collections.Generic.List{IFontPackage}"/></returns>
        Task<IBatchSingleResult<List<IFontPackage>>> GetFontPackagesBatchAsync(Batch batch);

        /// <summary>
        /// Gets the fonts available in branding center
        /// </summary>
        /// <returns>Site's <see cref="System.Collections.Generic.List{IFontPackage}"/></returns>
        IBatchSingleResult<List<IFontPackage>> GetFontPackagesBatch(Batch batch);

        /// <summary>
        /// Sets the branding center font with the given id on the Site
        /// </summary>
        /// <param name="fontId"></param>
        void SetFontPackage(string fontId);

        /// <summary>
        /// Sets the branding center font with the given id on the Site
        /// </summary>
        /// <param name="fontId"></param>
        Task SetFontPackageAsync(string fontId);

        /// <summary>
        /// Sets the branding center font with the given id on the Site
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="fontId"></param>
        void SetFontPackageBatch(Batch batch, string fontId);

        /// <summary>
        /// Sets the branding center font with the given id on the Site
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="fontId"></param>
        Task SetPackageBatchAsync(Batch batch, string fontId);

        #endregion Branding Center FontPackages

        #region SiteFontPackages

        /// <summary>
        /// Gets the fonts installed on the site
        /// </summary>
        /// <returns>Site's <see cref="System.Collections.Generic.List{IFontPackage}"/></returns>
        Task<List<IFontPackage>> GetSiteFontPackagesAsync();

        /// <summary>
        /// Gets the fonts installed on the site
        /// </summary>
        /// <returns>Site's <see cref="System.Collections.Generic.List{IFontPackage}"/></returns>
        List<IFontPackage> GetSiteFontPackages();

        /// <summary>
        /// Gets the fonts installed on the site
        /// </summary>
        /// <returns>Site's <see cref="System.Collections.Generic.List{IFontPackage}"/></returns>
        Task<IBatchSingleResult<List<IFontPackage>>> GetSiteFontPackagesBatchAsync(Batch batch);

        /// <summary>
        /// Gets the fonts installed on the site
        /// </summary>
        /// <returns>Site's <see cref="System.Collections.Generic.List{IFontPackage}"/></returns>
        IBatchSingleResult<List<IFontPackage>> GetSiteFontPackagesBatch(Batch batch);

        #endregion SiteFontPackages
    }
}
