using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Content Type object of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(ContentType))]
    public interface IContentType : IDataModel<IContentType>, IDataModelGet<IContentType>, IDataModelLoad<IContentType>, IDataModelUpdate, IDataModelDelete, IQueryableDataModel
    {

        #region Properties

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
        public string SchemaXml { get; }

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

        /// <summary>
        /// The unique identifier of the client-side component defined with SharePoint Framework
        /// </summary>
        public string NewFormClientSideComponentId { get; set; }

        /// <summary>
        /// This property is only used when a NewFormClientSideComponentId is specified. It is optional.
        /// If non-empty, the string must contain a JSON object with custom initialization properties
        /// whose format and meaning are defined by the client-side component. 
        /// </summary>
        public string NewFormClientSideComponentProperties { get; set; }

        /// <summary>
        /// The unique identifier of the client-side component defined with SharePoint Framework
        /// </summary>
        public string EditFormClientSideComponentId { get; set; }

        /// <summary>
        /// This property is only used when a EditFormClientSideComponentId is specified. It is optional.
        /// If non-empty, the string must contain a JSON object with custom initialization properties
        /// whose format and meaning are defined by the client-side component. 
        /// </summary>
        public string EditFormClientSideComponentProperties { get; set; }

        /// <summary>
        /// The unique identifier of the client-side component defined with SharePoint Framework
        /// </summary>
        public string DisplayFormClientSideComponentId { get; set; }

        /// <summary>
        /// This property is only used when a DisplayFormClientSideComponentId is specified. It is optional.
        /// If non-empty, the string must contain a JSON object with custom initialization properties
        /// whose format and meaning are defined by the client-side component. 
        /// </summary>
        public string DisplayFormClientSideComponentProperties { get; set; }

        /// <summary>
        /// Gets the collection of field links of the Content Type.
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IFieldLinkCollection FieldLinks { get; }

        /// <summary>
        /// Gets the collection of fields of the Content Type.
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IFieldCollection Fields { get; }

        /// <summary>
        /// A special property used to add an asterisk to a $select statement
        /// </summary>
        public object All { get; }

        #region To implement
        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IUserResource DescriptionResource { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IUserResource NameResource { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IContentType Parent { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IWorkflowAssociationCollection WorkflowAssociations { get; }
        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Publishes a content type from the hub to the sites in the SharePoint environment
        /// </summary>
        /// <returns></returns>
        Task PublishAsync();

        /// <summary>
        /// Publishes a content type from the hub to the sites in the SharePoint environment
        /// </summary>
        void Publish();

        /// <summary>
        /// Unpublishes a content type from the hub to the sites in the SharePoint environment
        /// </summary>
        /// <returns></returns>
        Task UnpublishAsync();

        /// <summary>
        /// Unublishes a content type from the hub to the sites in the SharePoint environment
        /// </summary>
        void Unpublish();

        /// <summary>
        /// Checks if a content type is published from the hub to the sites in the SharePoint environment
        /// </summary>
        /// <returns></returns>
        Task<bool> IsPublishedAsync();

        /// <summary>
        /// Checks if a content type is published from the hub to the sites in the SharePoint environment
        /// </summary>
        bool IsPublished();

        /// <summary>
        /// Returns the content type as a document set
        /// </summary>
        /// <returns>The content type as a document set</returns>
        Task<IDocumentSet> AsDocumentSetAsync();

        /// <summary>
        /// Returns the content type as a document set
        /// </summary>
        /// <returns>The content type as a document set</returns>
        IDocumentSet AsDocumentSet();

        /// <summary>
        /// Adds a field to the content type
        /// </summary>
        /// <param name="field"><see cref="IField"/> to add to this content type</param>
        /// <returns></returns>
        Task AddFieldAsync(IField field);

        /// <summary>
        /// Adds a field to the content type
        /// </summary>
        /// <param name="field"><see cref="IField"/> to add to this content type</param>
        /// <returns></returns>
        void AddField(IField field);

        #endregion
    }
}
