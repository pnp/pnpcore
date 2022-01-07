using System.IO;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options to configure a the site header chrome
    /// </summary>
    public interface IHeaderOptions
    {
        /// <summary>
        /// Gets or sets the value of the header layout.
        /// </summary>
        HeaderLayoutType Layout { get; set; }

        /// <summary>
        /// Gets or sets the value of the header emphasis.
        /// </summary>
        VariantThemeType Emphasis { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the title in header is hidden on the site.
        /// </summary>
        bool HideTitle { get; set; }

        /// <summary>
        /// Gets or sets the logo alignment of the site.
        /// </summary>
        public LogoAlignment LogoAlignment { get; set; }

        /// <summary>
        /// Sets the site's logo to the provided image.  For group connected sites calling this method is 
        /// equal to calling SetSiteLogoThumbnail as logo and logo thumbnail are both set the same.
        /// </summary>
        /// <param name="fileName">Name of your image file</param>
        /// <param name="content">The contents of the file</param>
        /// <param name="overwrite">Indicates whether the file should be overwritten if already existing.</param>
        /// <returns></returns>
        Task SetSiteLogoAsync(string fileName, Stream content, bool overwrite = false);

        /// <summary>
        /// Sets the site's logo to the provided image. For group connected sites calling this method is 
        /// equal to calling SetSiteLogoThumbnail as logo and logo thumbnail are both set the same.
        /// </summary>
        /// <param name="fileName">Name of your image file</param>
        /// <param name="content">The contents of the file</param>
        /// <param name="overwrite">Indicates whether the file should be overwritten if already existing.</param>
        /// <returns></returns>
        void SetSiteLogo(string fileName, Stream content, bool overwrite = false);

        /// <summary>
        /// Sets the out of the box site's logo again. For group connected sites calling this method is 
        /// equal to calling ResetSiteLogoThumbnail as logo and logo thumbnail are both set the same.
        /// </summary>
        /// <returns></returns>
        Task ResetSiteLogoAsync();

        /// <summary>
        /// Sets the out of the box site's logo again. For group connected sites calling this method is 
        /// equal to calling ResetSiteLogoThumbnail as logo and logo thumbnail are both set the same.
        /// </summary>
        /// <returns></returns>
        void ResetSiteLogo();

        /// <summary>
        /// Sets the site's logo thumbnail to the provided image.
        /// </summary>
        /// <param name="fileName">Name of your image file</param>
        /// <param name="content">The contents of the file. Image should in a square aspect ratio</param>
        /// <param name="overwrite">Indicates whether the file should be overwritten if already existing.</param>
        /// <returns></returns>
        Task SetSiteLogoThumbnailAsync(string fileName, Stream content, bool overwrite = false);

        /// <summary>
        /// Sets the site's logo thumbnail to the provided image. For group connected sites setting the logo thumbnail
        /// will also automatically update the site logo.
        /// </summary>
        /// <param name="fileName">Name of your image file</param>
        /// <param name="content">The contents of the file. Image should in a square aspect ratio</param>
        /// <param name="overwrite">Indicates whether the file should be overwritten if already existing.</param>
        /// <returns></returns>
        void SetSiteLogoThumbnail(string fileName, Stream content, bool overwrite = false);

        /// <summary>
        /// Sets the out of the box site's logo thumbnail again. For group connected sites we'll default to using
        /// siteassets/__siteIcon__.jpg if that still exists.
        /// </summary>
        /// <returns></returns>
        Task ResetSiteLogoThumbnailAsync();

        /// <summary>
        /// Sets the out of the box site's logo thumbnail again. For group connected sites we'll default to using
        /// siteassets/__siteIcon__.jpg if that still exists.
        /// </summary>
        /// <returns></returns>
        void ResetSiteLogoThumbnail();

        /// <summary>
        /// Sets the site's header background image. Only can be called when the header layout is set to extended.
        /// </summary>
        /// <param name="fileName">Name of your image file</param>
        /// <param name="content">The contents of the file</param>
        /// <param name="focalX">X axis focal point for the header image</param>
        /// <param name="focalY">Y axis focal point for the header image</param>
        /// <param name="overwrite">Indicates whether the file should be overwritten if already existing.</param>
        /// <returns></returns>
        Task SetHeaderBackgroundImageAsync(string fileName, Stream content, double focalX = 0, double focalY = 0, bool overwrite = false);

        /// <summary>
        /// Sets the site's header background image. Only can be called when the header layout is set to extended.
        /// </summary>
        /// <param name="fileName">Name of your image file</param>
        /// <param name="content">The contents of the file</param>
        /// <param name="focalX">X axis focal point for the header image</param>
        /// <param name="focalY">Y axis focal point for the header image</param>
        /// <param name="overwrite">Indicates whether the file should be overwritten if already existing.</param>
        /// <returns></returns>
        void SetHeaderBackgroundImage(string fileName, Stream content, double focalX = 0, double focalY = 0, bool overwrite = false);

        /// <summary>
        /// Clears the header background image
        /// </summary>
        /// <returns></returns>
        Task ClearHeaderBackgroundImageAsync();

        /// <summary>
        /// Clears the header background image
        /// </summary>
        /// <returns></returns>
        void ClearHeaderBackgroundImage();
    }
}
