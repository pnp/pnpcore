using System.IO;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options to configure a the site footer chrome.
    /// </summary>
    public interface IFooterOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies whether the footer is enabled on the site.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the value of the footer layout.
        /// </summary>
        FooterLayoutType Layout { get; set; }

        /// <summary>
        /// Gets or sets the value of the footer emphasis.
        /// </summary>
        FooterVariantThemeType Emphasis { get; set; }

        /// <summary>
        /// The footer display name
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// Sets the footer's logo to the provided image.
        /// </summary>
        /// <param name="fileName">Name of your image file</param>
        /// <param name="content">The contents of the file</param>
        /// <param name="overwrite">Indicates whether the file should be overwritten if already existing.</param>
        /// <returns></returns>
        Task SetLogoAsync(string fileName, Stream content, bool overwrite = false);

        /// <summary>
        /// Sets the footer's logo to the provided image.
        /// </summary>
        /// <param name="fileName">Name of your image file</param>
        /// <param name="content">The contents of the file</param>
        /// <param name="overwrite">Indicates whether the file should be overwritten if already existing.</param>
        /// <returns></returns>
        void SetLogo(string fileName, Stream content, bool overwrite = false);

        /// <summary>
        /// Clears the footer logo
        /// </summary>
        /// <returns></returns>
        Task ClearLogoAsync();

        /// <summary>
        /// Clears the footer logo
        /// </summary>
        /// <returns></returns>
        void ClearLogo();
    }
}
