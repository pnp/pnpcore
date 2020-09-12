namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Footer Variant theme type
    /// (see https://github.com/microsoft/fluentui/tree/master/packages/variants)
    /// </summary>
    public enum VariantThemeType
    {
        /// <summary>
        /// no emphasis - appears as normal ( Value = 0 )
        /// </summary>
        None = 0,
        /// <summary>
        /// neutral variant - light shade of original background as background ( Value = 1 )
        /// </summary>
        Neutral = 1,
        /// <summary>
        /// soft variant - light tint of the primary color as background ( Value = 2 )
        /// </summary>
        Soft = 2,
        /// <summary>
        /// strong variant - primary color as background, text uses original background color; white on brand blue by default ( Value = 3 )
        /// </summary>
        Strong = 3
    }
}
