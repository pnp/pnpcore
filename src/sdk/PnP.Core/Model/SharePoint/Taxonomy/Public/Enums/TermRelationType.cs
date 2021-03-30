namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Type of relationship between two terms
    /// </summary>
    public enum TermRelationType
    {
        /// <summary>
        /// In a pin relationship, a term can be pinned under a different term in a different term set. 
        /// In a pinned relationship, new children to the term can only be added in the term set in which the term was created. 
        /// Any change in the hierarchy under the term is reflected across the sets in which the term was pinned.
        /// </summary>
        Pin,

        /// <summary>
        /// The reuse relationship is similar to the pinned relationship except that changes to the reused term can be made from any hierarchy in which the term is reused. 
        /// Also, a change in hierarchy made to the reused term does not get reflected in the other term sets in which the term is reused.
        /// </summary>
        Reuse
    }
}