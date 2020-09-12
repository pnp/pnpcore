namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Variants documentation: https://github.com/OfficeDev/office-ui-fabric-react/tree/master/packages/variants
    /// Note that this definition is similar to SPVariantThemeType, but with the strong and none variants switched.
    /// This is because we want strong to be the default for footer.
    /// </summary>
    public enum FooterVariantThemeType
    {
        /// <summary>
        /// strong variant - primary color as background, text uses original background color; white on brand blue by default ( Value = 0 )
        /// </summary>
        Strong = 0,
        /// <summary>
        /// neutral variant - light shade of original background as background ( Value = 1 )
        /// </summary>
        Neutral = 1,
        /// <summary>
        /// soft variant - light tint of the primary color as background ( Value = 2 )
        /// </summary>
        Soft = 2,
        /// <summary>
        /// no emphasis - appears as normal ( Value = 3 )
        /// </summary>
        None = 3
    }
}
