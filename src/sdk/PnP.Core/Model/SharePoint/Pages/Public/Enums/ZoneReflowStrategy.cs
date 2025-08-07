namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the strategy used to reflow web parts within a zone.
    /// </summary>
    /// <remarks>A reflow strategy determines the arrangement of web parts when resizing or reorganizing a
    /// zone. Use <see cref="TopToDown"/> to arrange web parts vertically, or <see cref="LeftToRight"/> to arrange them
    /// horizontally.</remarks>
    public enum ZoneReflowStrategy
    {
        /// <summary>
        /// webparts flow from top to bottom
        /// </summary>
        TopToDown,

        /// <summary>
        /// webparts from from left to right
        /// </summary>
        LeftToRight
    }
}
