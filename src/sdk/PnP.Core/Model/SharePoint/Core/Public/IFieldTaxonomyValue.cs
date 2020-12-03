using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a taxonomy field value
    /// </summary>
    public interface IFieldTaxonomyValue : IFieldValue
    {
        /// <summary>
        /// Taxonomy label
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Taxonomy term id
        /// </summary>
        public Guid TermId { get; set; }
    }
}
