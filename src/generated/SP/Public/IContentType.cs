using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ContentType object
    /// </summary>
    [ConcreteType(typeof(ContentType))]
    public interface IContentType : IDataModel<IContentType>, IDataModelGet<IContentType>, IDataModelUpdate, IDataModelDelete
    {

        #region Existing properties

        /// <summary>
        /// To update...
        /// </summary>
        public string ClientFormCustomFormatter { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DisplayFormTemplateName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DisplayFormUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DocumentTemplate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DocumentTemplateUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string EditFormTemplateName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string EditFormUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string JSLink { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string MobileDisplayFormUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string MobileEditFormUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string MobileNewFormUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string NewFormTemplateName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string NewFormUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SchemaXml { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SchemaXmlWithResourceTokens { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Sealed { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string StringId { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFieldLinkCollection FieldLinks { get; }

        #endregion

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public IUserResource DescriptionResource { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFieldCollection Fields { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUserResource NameResource { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IContentType Parent { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IWorkflowAssociationCollection WorkflowAssociations { get; }

        #endregion

    }
}
