using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a UserCustomAction object
    /// </summary>
    [ConcreteType(typeof(UserCustomAction))]
    public interface IUserCustomAction : IDataModel<IUserCustomAction>, IDataModelGet<IUserCustomAction>, IDataModelUpdate, IDataModelDelete
    {

        #region Existing properties

        /// <summary>
        /// To update...
        /// </summary>
        public Guid ClientSideComponentId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ClientSideComponentProperties { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string CommandUIExtension { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string HostProperties { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string RegistrationId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int RegistrationType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int Scope { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ScriptBlock { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ScriptSrc { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string VersionOfUserCustomAction { get; }

        #endregion

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public IUserResource DescriptionResource { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUserResource TitleResource { get; }

        #endregion

    }
}
