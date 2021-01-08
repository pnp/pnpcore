namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Describes whether a field is indexed, and whether the data
    /// in the index is complete
    /// </summary>
    public enum FieldIndexStatus : int
    {
        /// <summary>
        /// The field is not indexed.
        /// </summary>
        None,

        /// <summary>
        /// The field is indexed.
        /// </summary>
        Indexed,

        /// <summary>
        /// The field index definition has been created, but its data is in the process of being populated.
        /// </summary>
        Enabling,

        /// <summary>
        /// The field index definition has been deleted, but its data is in the process of being removed.
        /// </summary>
        Disabling
    }
}
