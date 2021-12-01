using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Hodlds properties for Taxonomy filed
    /// </summary>
    internal sealed class TaxonomyFieldCreationOptions : FieldCreationOptions
    {
        private bool multiValue;

        /// <summary>
        /// Allows multiple values for Taxonomy field
        /// </summary>
        public bool MultiValue
        {
            get
            {
                return multiValue;
            }
            set
            {
                if (value)
                {
                    FieldType = "TaxonomyFieldTypeMulti";
                    SetAttribute("Mult", "TRUE");
                }
                else
                {
                    FieldType = "TaxonomyFieldType";
                    SetAttribute("Mult", "FALSE");
                }

                multiValue = value;
            }
        }

        /// <summary>
        /// Term store id
        /// </summary>
        public Guid TermStoreId { get; set; }

        /// <summary>
        /// TermSet id
        /// </summary>
        public Guid TermSetId { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public TaxonomyFieldCreationOptions() : base("TaxonomyFieldType")
        { }

    }

}
