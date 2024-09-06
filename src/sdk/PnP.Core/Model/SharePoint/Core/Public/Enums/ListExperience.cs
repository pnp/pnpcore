namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// List experience, determines how a list is presented (modern or classic)
    /// </summary>
    public enum ListExperience
    {
        /// <summary>
        ///  SPO will automatically define the right experience based on the settings of the current list, it is the default value.
        /// </summary>
        Auto = 0,
        
        /// <summary>
        /// The Modern experience will be forced for the current list.
        /// </summary>
        NewExperience = 1,

        /// <summary>
        /// The Classic experience will be forced for the current list.
        /// </summary>
        ClassicExperience = 2,
    }
}
