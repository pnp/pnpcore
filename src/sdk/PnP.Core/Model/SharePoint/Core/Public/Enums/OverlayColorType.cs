namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the available overlay color types that can be applied to header or footer.
    /// </summary>
    /// <remarks>This enumeration provides options for selecting predefined overlay colors, including solid
    /// colors and gradient styles. The values can be used to customize the appearance of UI elements or graphical
    /// overlays.</remarks>
    public enum OverlayColorType
    {
        /// <summary>
        /// -1 = not set
        /// </summary>
        None = -1,
        /// <summary>
        /// 0 = white
        /// </summary>
        White = 0,
        /// <summary>
        /// 1 = black
        /// </summary>
        Black = 1,
        /// <summary>
        /// 2 = light gradient
        /// </summary>
        LightGradient = 2,
        /// <summary>
        /// 3 = dark gradient
        /// </summary>
        DarkGradient = 3
    }
}
