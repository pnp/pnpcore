using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options for configuring a Taxonomy field
    /// </summary>
    public class FieldTaxonomyOptions : CommonFieldOptions
    {
        /// <summary>
        /// Term store id
        /// </summary>
        public Guid TermStoreId { get; set; }

        /// <summary>
        /// TermSet id
        /// </summary>
        public Guid TermSetId { get; set; }

    }
}
