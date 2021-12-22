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
    }
}
