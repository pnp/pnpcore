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
        public string Label { get; }

        /// <summary>
        /// Taxonomy term id
        /// </summary>
        public Guid TermId { get; }

        /// <summary>
        /// Loads a term into this <see cref="IFieldTaxonomyValue"/> object
        /// </summary>
        /// <param name="termId">Id of the term</param>
        /// <param name="label">Label of the term</param>
        /// <param name="wssId">Optionally specify the WssId value</param>
        public void LoadTerm(Guid termId, string label, int wssId = -1);

    }
}
