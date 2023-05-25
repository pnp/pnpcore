namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Options for updating a collection 
    /// </summary>
    public enum CollectionUpdateOptions
    {
        /// <summary>
        /// Add values to existing list. Value = 0.
        /// </summary>
        AddOnly = 0,
        /// <summary>
        /// Set exactly the passed values. Value = 1.
        /// </summary>
        SetExact = 1
    }
}
