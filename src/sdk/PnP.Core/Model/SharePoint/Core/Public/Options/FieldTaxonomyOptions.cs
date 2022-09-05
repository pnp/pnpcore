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

        /// <summary>
        /// Default value set on single value taxonomy field
        /// </summary>
        public ITerm DefaultValue { get; set; }

        /// <summary>
        /// Default values set on a a multi value taxonomy field
        /// </summary>
        public System.Collections.Generic.List<ITerm> DefaultValues { get; set; }

        /// <summary>
        /// Defines whether the provisioned field allows for additions to the connected term set
        /// </summary>
        public bool OpenTermSet { get; set; } = false;
    }
}
