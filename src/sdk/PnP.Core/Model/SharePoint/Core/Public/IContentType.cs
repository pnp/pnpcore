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
        public string StringId { get; set; }

        /// <summary>
        /// The unique ID of the Content Type as object
        /// </summary>
        public string Id { get; set; }

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
    }
}
