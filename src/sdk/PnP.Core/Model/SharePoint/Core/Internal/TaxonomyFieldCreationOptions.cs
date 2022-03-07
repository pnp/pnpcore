using PnP.Core.Services.Core.CSOM;
using System;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Hodlds properties for Taxonomy filed
    /// </summary>
    internal sealed class TaxonomyFieldCreationOptions : FieldCreationOptions
    {
        private bool multiValue;
        private ITerm defaultValue;
        private System.Collections.Generic.List<ITerm> defaultValues;

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
        /// Default value for this taxonomy field
        /// </summary>
        public ITerm DefaultValue
        {
            get
            {
                return defaultValue;  
            }
            set
            {
                if (value != null)
                {
                    var defaultString = $"-1;#{value.Labels.First(p => p.IsDefault == true).Name}|{value.Id}";
                    SetChildXmlNode("DefaultValue", $"<Default>{CsomHelper.XmlString(defaultString)}</Default>");
                }
                defaultValue = value;
            }
        }

        public System.Collections.Generic.List<ITerm> DefaultValues
        {
            get
            {
                return defaultValues;
            }
            set
            {
                if (value != null && value.Count > 0)
                {
                    string defaultString = "";
                    foreach(var term in value)
                    {
                        var defaultTermString = $"-1;#{term.Labels.First(p => p.IsDefault == true).Name}|{term.Id}";
                        if (!string.IsNullOrEmpty(defaultString))
                        {
                            defaultString = defaultString + ";#" + defaultTermString;
                        }
                        else
                        {
                            defaultString = defaultTermString;
                        }
                    }
                    SetChildXmlNode("DefaultValue", $"<Default>{CsomHelper.XmlString(defaultString)}</Default>");
                }

                defaultValues = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public TaxonomyFieldCreationOptions() : base("TaxonomyFieldType")
        { }

    }

}
