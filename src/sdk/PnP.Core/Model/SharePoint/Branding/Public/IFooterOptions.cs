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
        /// specifies the alignment of links in the footer
        /// </summary>
        FooterLinkAlignment LinkAlignment { get; set; }
        
        /// <summary>
        /// Specifies the available overlay color types that can be applied to header or footer.
        /// </summary>
        OverlayColorType OverlayColor { get; set; }
        /// <summary>
        /// Gets or sets the opacity level of the overlay [0-100].
        /// </summary>
        int OverlayOpacity { get; set; }

        /// <summary>
        /// Defines the possible directions for an overlay gradient layout.
        /// </summary>
        OverlayGradientDirectionType OverlayGradientDirection { get; set; }

        /// <summary>
        /// seesm to be alays -1
        /// </summary>
        int ColorIndexInLightMode { get; set; }

        /// <summary>
        /// seesm to be alays -1
        /// </summary>
        int ColorIndexInDarkMode { get; set; }

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


        /// <summary>
        /// Sets the site's footer background image.
        /// </summary>
        /// <param name="fileName">Name of your image file</param>
        /// <param name="content">The contents of the file</param>
        /// <param name="focalX">X axis focal point for the footer image</param>
        /// <param name="focalY">Y axis focal point for the footer image</param>
        /// <param name="overwrite">Indicates whether the file should be overwritten if already existing.</param>
        /// <returns></returns>
        Task SetFooterBackgroundImageAsync(string fileName, Stream content, double focalX = 0, double focalY = 0, bool overwrite = false);

        /// <summary>
        /// Sets the site's footer background image. 
        /// </summary>
        /// <param name="fileName">Name of your image file</param>
        /// <param name="content">The contents of the file</param>
        /// <param name="focalX">X axis focal point for the footer image</param>
        /// <param name="focalY">Y axis focal point for the footer image</param>
        /// <param name="overwrite">Indicates whether the file should be overwritten if already existing.</param>
        /// <returns></returns>
        void SetFooterBackgroundImage(string fileName, Stream content, double focalX = 0, double focalY = 0, bool overwrite = false);

        /// <summary>
        /// Clears the footer background image
        /// </summary>
        /// <returns></returns>
        Task ClearFooterBackgroundImageAsync();

        /// <summary>
        /// Clears the footer background image
        /// </summary>
        /// <returns></returns>
        void ClearFooterBackgroundImage();
    }
}
