namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Content Type object of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(ContentType))]
    public interface IContentType : IDataModel<IContentType>, IDataModelUpdate, IDataModelDelete
    {
        /// <summary>
        /// The unique ID of the Content Type as string
        /// </summary>
        public string StringId { get; }

        /// <summary>
        /// The unique ID of the Content Type as object
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets or Sets the Client Form Custom Formatter of the Content Type
        /// </summary>
        public string ClientFormCustomFormatter { get; set; }

        /// <summary>
        /// Gets or Sets the description of the Content Type
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or Sets the Display Form template name of the Content Type
        /// </summary>
        public string DisplayFormTemplateName { get; set; }

        /// <summary>
        /// Gets or Sets the Display Form URL of the Content Type
        /// </summary>
        public string DisplayFormUrl { get; set; }

        /// <summary>
        /// Gets or Sets the Document Template of the Content Type
        /// </summary>
        public string DocumentTemplate { get; set; }

        /// <summary>
        /// Gets or Sets the DocumentTemplate URL of the Content Type
        /// </summary>
        public string DocumentTemplateUrl { get; set; }

        /// <summary>
        /// Gets or Sets the Edit Form template name of the Content Type
        /// </summary>
        public string EditFormTemplateName { get; set; }

        /// <summary>
        /// Gets or Sets the Edit Form URL of the Content Type
        /// </summary>
        public string EditFormUrl { get; set; }

        /// <summary>
        /// Gets or Sets the group of the Content Type
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Gets or Sets that specifies whether the Content Type is hidden
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets or Sets the JS Link of the Content Type
        /// </summary>
        public string JSLink { get; set; }

        /// <summary>
        /// Gets or Sets the Mobile Display Form URL of the Content Type
        /// </summary>
        public string MobileDisplayFormUrl { get; set; }

        /// <summary>
        /// Gets or Sets the Mobile Edit Form URL of the Content Type
        /// </summary>
        public string MobileEditFormUrl { get; set; }

        /// <summary>
        /// Gets or Sets the Mobile New Form URL of the Content Type
        /// </summary>
        public string MobileNewFormUrl { get; set; }

        /// <summary>
        /// Gets or Sets the name of the Content Type
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the New Form template's name of the Content Type
        /// </summary>
        public string NewFormTemplateName { get; set; }

        /// <summary>
        /// Gets or Sets the New Form URL of the Content Type
        /// </summary>
        public string NewFormUrl { get; set; }


        /// <summary>
        /// Gets or Sets whether the Content Type is read only
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Gets or Sets the Schema XML of the Content Type
        /// </summary>
        public string SchemaXml { get; set; }

        /// <summary>
        /// Gets or Sets the Schema XML with resource tokens of the Content Type
        /// </summary>
        public string SchemaXmlWithResourceTokens { get; set; }

        /// <summary>
        /// Gets or Sets the scope of the Content Type
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Gets or Sets whether the Content Type is sealed
        /// </summary>
        public bool Sealed { get; set; }

        // TODO To implement
        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IUserResource DescriptionResource { get; }

        /// <summary>
        /// Gets the collection of field links of the Content Type.
        /// </summary>
        public IFieldLinkCollection FieldLinks { get; }

        // TODO To implement
        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IFieldCollection Fields { get; }

        // TODO To implement
        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IUserResource NameResource { get; }

        // TODO To implement
        // TODO How to handle this ? (Parent already exists in DataModelBase)
        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IContentType Parent { get; }

        // TODO To implement
        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IWorkflowAssociationCollection WorkflowAssociations { get; }

    }
}
