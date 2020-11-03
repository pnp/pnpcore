namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Layout of the page header
    /// </summary>
    public enum PageHeaderLayoutType
    {
        /// <summary>
        /// Layout using a full width image as header background with a title
        /// </summary>
        FullWidthImage,
        
        /// <summary>
        /// No image in the header background, only a title
        /// </summary>
        NoImage,

        /// <summary>
        /// Header based upon a title shown in a color block. Can still have a background image
        /// </summary>
        ColorBlock,

        /// <summary>
        /// Header based upon a title shown as an overlap. Can still have a background image
        /// </summary>
        CutInShape,
    }
}
