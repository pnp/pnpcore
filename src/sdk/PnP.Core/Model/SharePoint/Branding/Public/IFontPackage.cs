namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Returned by
    /// /_api/OutOfBoxFontPackages
    /// /_api/SiteFontPackages
    /// /_api/FontPackages
    /// </summary>
    public interface IFontPackage
    {
        /// <summary>
        /// Font Package ID example /_api/OutOfBoxFontPackages/GetById('5f955493-db94-446f-93bf-2c7567861329')
        /// </summary>
        string ID { get; set; }

        /// <summary>
        /// dont show this font in the font picker
        /// </summary>
        bool IsHidden { get; set; }
        
        /// <summary>
        /// IsValid
        /// </summary>
        bool IsValid { get; set; }

        /// <summary>
        /// json-string containg settings for the font package
        /// </summary>
        string PackageJson { get; set; }

        /// <summary>
        /// 0 = Branding Center, 1 = out of the box font
        /// </summary>
        int Store { get; set; }

        /// <summary>
        /// display name of the font package
        /// </summary>
        string Title { get; set; }
    }
}
