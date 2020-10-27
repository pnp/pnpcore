using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ListTemplate object
    /// </summary>
    [ConcreteType(typeof(ListTemplate))]
    public interface IListTemplate : IDataModel<IListTemplate>, IDataModelGet<IListTemplate>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowsFolderCreation { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int BaseType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid FeatureId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string InternalName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsCustomTemplate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool OnQuickLaunch { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ListTemplateTypeKind { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Unique { get; set; }

        #endregion

    }
}
