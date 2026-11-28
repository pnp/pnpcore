using System;

namespace PnP.Core.Modernization.Entities
{
    [Serializable]
    public class TermData
    {
        /// <summary>
        /// Term Guid
        /// </summary>
        public Guid TermGuid { get; set; }

        /// <summary>
        /// Term Label
        /// </summary>
        public string TermLabel { get; set; }

        /// <summary>
        /// Term Path
        /// </summary>
        public string TermPath { get; set; }

        /// <summary>
        /// Term Set ID
        /// </summary>
        public Guid TermSetId { get; set; }

        /// <summary>
        /// Marks the term data validation against the term store
        /// </summary>
        public bool IsTermResolved { get; set; }

        /// <summary>
        /// Is the term a source term
        /// </summary>
        public bool IsSourceTerm { get; set; }

    }
}
