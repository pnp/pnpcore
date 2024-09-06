using PnP.Core.Services.Core.CSOM;
using System;
using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Class that describes the field creation options
    /// </summary>
    internal class FieldCreationOptions
    {
        /// <summary>
        /// Guid of the field
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Field display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Field internal name
        /// </summary>
        public string InternalName { get; set; }

        /// <summary>
        /// Adds fields to default view if value is true.
        /// </summary>
        public bool AddToDefaultView { get; set; }

        /// <summary>
        /// List of additional properties that need to be applied to the field on creation
        /// </summary>
        public Dictionary<string, string> AdditionalAttributes { get; } = new Dictionary<string, string>();

        /// <summary>
        /// List of additional child nodes that need to be included in the CAML field on creation
        /// </summary>
        public Dictionary<string, string> AdditionalChildNodes { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Type of the field
        /// </summary>
        public string FieldType { get; internal set; }

        /// <summary>
        /// Group of the field
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Specifies filds is required to enter vlaue or not.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Ignored currently for SP2016
        /// </summary>
        public Guid ClientSideComponentId { get; set; }

        /// <summary>
        /// Ignored currently for SP2016
        /// </summary>
        public string ClientSideComponentProperties { get; set; }

        /// <summary>
        /// An <see cref="AddFieldOptionsFlags"/> flag that specifies the field options to be applied during add
        /// </summary>
        public AddFieldOptionsFlags Options { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldType">Type of the field</param>
        public FieldCreationOptions(string fieldType)
        {
            FieldType = fieldType;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldType">Type of the field</param>
        public FieldCreationOptions(FieldType fieldType) : this(fieldType.ToString())
        {
        }

        /// <summary>
        /// Imports settings from the <see cref="CommonFieldOptions"/> configuration into the generic <see cref="FieldCreationOptions"/> model
        /// </summary>
        /// <param name="common">Set of field creation options to transform</param>
        /// <param name="title">Field title</param>
        internal void ImportFromCommonFieldOptions(string title, CommonFieldOptions common)
        {

            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (common != null && common.Id.HasValue)
            {
                Id = common.Id.Value;
            }
            else
            {
                Id = Guid.NewGuid();
            }

            DisplayName = title;
            InternalName = title;
            if (common == null)
            {
                return;
            }

            // In a field internal name is provided then use that one
            InternalName = common.InternalName ?? title;
            // Ensure the AddFieldInternalNameHint is set
            if (!string.IsNullOrEmpty(common.InternalName))
            {
                common.Options |= AddFieldOptionsFlags.AddFieldInternalNameHint;
            }

            AddToDefaultView = common.AddToDefaultView;
            Options = common.Options;

            if (common.Required.HasValue)
            {
                Required = common.Required.Value;
            }

            if (!string.IsNullOrEmpty(common.Group))
            {
                Group = common.Group;
            }

            if (!string.IsNullOrEmpty(common.Description))
            {
                SetAttribute("Description", common.Description);
            }

            if (common.EnforceUniqueValues.HasValue)
            {
                SetAttribute("EnforceUniqueValues", common.EnforceUniqueValues.Value.ToString().ToUpper());
            }

            if (common.Hidden.HasValue)
            {
                SetAttribute("Hidden", common.Hidden.Value.ToString().ToUpper());
            }

            if (common.Indexed.HasValue)
            {
                SetAttribute("Indexed", common.Indexed.Value.ToString().ToUpper());
            }

            if (!string.IsNullOrEmpty(common.DefaultFormula))
            {
                SetChildXmlNode("DefaultFormula", $"<DefaultFormula>{CsomHelper.XmlString(common.DefaultFormula)}</DefaultFormula>");
            }

            if (!string.IsNullOrEmpty(common.ValidationFormula) && !string.IsNullOrEmpty(common.ValidationMessage))
            {
                SetChildXmlNode("Validation", $"<Validation Message=\"{CsomHelper.XmlString(common.ValidationMessage, true)}\">{CsomHelper.XmlString(common.ValidationFormula)}</Validation>");
            }

            if (common.ShowInEditForm.HasValue)
            {
                SetAttribute("ShowInEditForm", common.ShowInEditForm.Value.ToString().ToUpper());
            }

            if (common.ShowInViewForms.HasValue)
            {
                SetAttribute("ShowInViewForms", common.ShowInViewForms.Value.ToString().ToUpper());
            }

            if (common.ShowInNewForm.HasValue)
            {
                SetAttribute("ShowInNewForm", common.ShowInNewForm.Value.ToString().ToUpper());
            }

            if (!string.IsNullOrEmpty(common.CustomFormatter))
            {
                SetAttribute("CustomFormatter", common.CustomFormatter);
            }
        }

        internal void SetAttribute(string key, string value)
        {
            if (!AdditionalAttributes.ContainsKey(key))
            {
                AdditionalAttributes.Add(key, value);
            }
            else
            {
                AdditionalAttributes[key] = value;
            }
        }

        internal void RemoveAttribute(string key)
        {
            if (AdditionalAttributes.ContainsKey(key))
            {
                AdditionalAttributes.Remove(key);
            }
        }

        internal void SetChildNode(string key, string value)
        {
            if (!AdditionalChildNodes.ContainsKey(key))
            {
                AdditionalChildNodes.Add(key, value);
            }
            else
            {
                AdditionalChildNodes[key] = value;
            }
        }

        internal void SetChildXmlNode(string key, string value)
        {
            string keyToUse = $"Xml:{key}";
            if (!AdditionalChildNodes.ContainsKey(keyToUse))
            {
                AdditionalChildNodes.Add(keyToUse, value);
            }
            else
            {
                AdditionalChildNodes[keyToUse] = value;
            }
        }

        internal void RemoveChildNode(string key)
        {
            if (AdditionalChildNodes.ContainsKey(key))
            {
                AdditionalChildNodes.Remove(key);
            }
        }
    }
}
