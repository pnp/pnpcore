using PnP.Core.Provisioning.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Domain Object used in the Provisioning template that defines a Content Type
    /// <seealso>
    ///     <cref>https://msdn.microsoft.com/en-us/library/office/ms463449.aspx</cref>
    /// </seealso>
    /// </summary>
    public partial class ContentType : BaseModel, IEquatable<ContentType>
    {
        #region Private Members
        private string _id;
        private FieldRefCollection _fieldRefs;
        #endregion

        #region Properties

        /// <summary>
        /// The Id of the Content Type
        /// </summary>
        public string Id { get { return _id; } set { _id = value; } }

        /// <summary>
        /// The name of the Content Type
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description of the Content Type
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The group name of the content type
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// The FieldRefs entries of the List Instance
        /// </summary>
        public FieldRefCollection FieldRefs
        {
            get { return this._fieldRefs; }
            private set { this._fieldRefs = value; }
        }

        /// <summary>
        /// True to define the content type as hidden. If you define a content type as hidden, SharePoint Foundation does not display that content type on the New button in list views. 
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// True to prevent changes to this content type. You cannot change the value of this attribute through the user interface, but you can change it in code if you have sufficient rights. You must have site collection administrator rights to unseal a content type. 
        /// </summary>
        public bool Sealed { get; set; }

        /// <summary>
        /// True to specify that the content type cannot be edited without explicitly removing the read-only setting. This can be done either in the user interface or in code. 
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// True to overwrite an existing content type with the same ID.
        /// </summary>
        public bool Overwrite { get; set; }

        /// <summary>
        /// Specifies the document template for the content type
        /// </summary>
        public string DocumentTemplate { get; set; }

        /// <summary>
        /// Specifies the properties of the DocumentSet Template if the ContentType defines a DocumentSet
        /// </summary>
        public DocumentSetTemplate DocumentSetTemplate { get; set; }

        /// <summary>
        /// Specifies the URL of a custom display form to use for list items that have been assigned the content type
        /// </summary>
        public String DisplayFormUrl { get; set; }

        /// <summary>
        /// Specifies the URL of a custom edit form to use for list items that have been assigned the content type
        /// </summary>
        public String EditFormUrl { get; set; }

        /// <summary>
        /// Specifies the URL of a custom new form to use for list items that have been assigned the content type
        /// </summary>
        public String NewFormUrl { get; set; }

        /// <summary>
        /// Declares whether changes to the content type will be udpated on inherited content types, optional attribute
        /// </summary>
        public bool UpdateChildren { get; set; }

        /// <summary>
        /// Declares the ID of the SPFx Client Side Component to customize the Display Form of the Content Type.
        /// </summary>
        public string DisplayFormClientSideComponentId { get; set; }

        /// <summary>
        /// Defines the properties of the SPFx Client Side Component to customize the Display Form of the Content Type.
        /// </summary>
        public string DisplayFormClientSideComponentProperties { get; set; }

        /// <summary>
        /// Declares the ID of the SPFx Client Side Component to customize the New Form of the Content Type.
        /// </summary>
        public string NewFormClientSideComponentId { get; set; }

        /// <summary>
        /// Defines the properties of the SPFx Client Side Component to customize the New Form of the Content Type.
        /// </summary>
        public string NewFormClientSideComponentProperties { get; set; }

        /// <summary>
        /// Declares the ID of the SPFx Client Side Component to customize the Edit Form of the Content Type.
        /// </summary>
        public string EditFormClientSideComponentId { get; set; }

        /// <summary>
        /// Defines the properties of the SPFx Client Side Component to customize the Edit Form of the Content Type.
        /// </summary>
        public string EditFormClientSideComponentProperties { get; set; }

        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for ContentType class
        /// </summary>
        public ContentType()
        {
            _fieldRefs = new FieldRefCollection(this.ParentTemplate);
        }

        /// <summary>
        /// Constructor for ContentType class
        /// </summary>
        /// <param name="id">Content Type ID</param>
        /// <param name="name">Content Type Name</param>
        /// <param name="description">Content Type Description</param>
        /// <param name="group">Content Type Group</param>
        /// <param name="contenttypeSealed">Sealed Content Type</param>
        /// <param name="hidden">Hidden Content Type</param>
        /// <param name="readyonly">ReadOnly Content Type</param>
        /// <param name="documentTemplate">Document template of the content type</param>
        /// <param name="overwrite">Specify true to overwrite</param>
        /// <param name="fieldRefs">Content Type Field References</param>
        public ContentType(string id, string name, string description, string group, bool contenttypeSealed, bool hidden, bool readyonly, string documentTemplate, bool overwrite, IEnumerable<FieldRef> fieldRefs) :
            this()
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.Group = group;
            this.Hidden = hidden;
            this.Sealed = contenttypeSealed;
            this.Overwrite = overwrite;
            this.ReadOnly = readyonly;
            this.DocumentTemplate = documentTemplate;
            this.FieldRefs.AddRange(fieldRefs);
        }

        #endregion

        #region Comparison code

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Returns HashCode</returns>
        public override int GetHashCode()
        {
            return (String.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}|{16}|{17}|",
                (this.Id != null ? this.Id.GetHashCode() : 0),
                (this.Name != null ? this.Name.GetHashCode() : 0),
                (this.Description != null ? this.Description.GetHashCode() : 0),
                (this.Group != null ? this.Group.GetHashCode() : 0),
                this.Hidden.GetHashCode(),
                this.ReadOnly.GetHashCode(),
                this.Overwrite.GetHashCode(),
                this.Sealed.GetHashCode(),
                (this.DocumentTemplate != null ? this.DocumentTemplate.GetHashCode() : 0),
                (this.DocumentSetTemplate != null ? this.DocumentSetTemplate.GetHashCode() : 0),
                this.FieldRefs.Aggregate(0, (acc, next) => acc += (next != null ? next.GetHashCode() : 0)),
                this.UpdateChildren.GetHashCode(),
                (this.DisplayFormClientSideComponentId != null ? this.DisplayFormClientSideComponentId.GetHashCode() : 0),
                (this.DisplayFormClientSideComponentProperties != null ? this.DisplayFormClientSideComponentProperties.GetHashCode() : 0),
                (this.NewFormClientSideComponentId != null ? this.NewFormClientSideComponentId.GetHashCode() : 0),
                (this.NewFormClientSideComponentProperties != null ? this.NewFormClientSideComponentProperties.GetHashCode(): 0),
                (this.EditFormClientSideComponentId != null ? this.EditFormClientSideComponentId.GetHashCode() : 0),
                (this.EditFormClientSideComponentProperties != null ? this.EditFormClientSideComponentProperties.GetHashCode() : 0)
            ).GetHashCode());
        }

        /// <summary>
        /// Compares object with ContentType
        /// </summary>
        /// <param name="obj">Object that represents ContentType</param>
        /// <returns>true if the current object is equal to the ContentType</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ContentType))
            {
                return (false);
            }
            return (Equals((ContentType)obj));
        }

        /// <summary>
        /// Compares ContentType object based on Id, Name, Description, Group, Hidden, RedOnly, Overwrite, Sealed, DocumentTemplate, DocumentSetTemplate, FieldRefs, and UpdateChildren properties.
        /// </summary>
        /// <param name="other">ContentType object</param>
        /// <returns>true if the ContentType object is equal to the current object; otherwise, false.</returns>
        public bool Equals(ContentType other)
        {
            if (other == null)
            {
                return (false);
            }

            return (this.Id == other.Id &&
                    this.Name == other.Name &&
                    this.Description == other.Description &&
                    this.Group == other.Group &&
                    this.Hidden == other.Hidden &&
                    this.ReadOnly == other.ReadOnly &&
                    this.Overwrite == other.Overwrite &&
                    this.Sealed == other.Sealed &&
                    this.DocumentTemplate == other.DocumentTemplate &&
                    (this.DocumentSetTemplate != null ? this.DocumentSetTemplate.Equals(other.DocumentSetTemplate) : this.DocumentSetTemplate == null && other.DocumentSetTemplate == null ? true : false) &&
                    this.FieldRefs.DeepEquals(other.FieldRefs) &&
                    this.UpdateChildren == other.UpdateChildren &&
                    this.DisplayFormClientSideComponentId == other.DisplayFormClientSideComponentId &&
                    this.DisplayFormClientSideComponentProperties == other.DisplayFormClientSideComponentProperties &&
                    this.NewFormClientSideComponentId == other.NewFormClientSideComponentId &&
                    this.NewFormClientSideComponentProperties == other.NewFormClientSideComponentProperties &&
                    this.EditFormClientSideComponentId == other.EditFormClientSideComponentId &&
                    this.EditFormClientSideComponentProperties == other.EditFormClientSideComponentProperties
                );

        }

        #endregion
    }
}
