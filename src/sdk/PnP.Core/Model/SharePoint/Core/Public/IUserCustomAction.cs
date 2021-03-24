using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a UserCustomAction object
    /// </summary>
    [ConcreteType(typeof(UserCustomAction))]
    public interface IUserCustomAction : IDataModel<IUserCustomAction>, IDataModelGet<IUserCustomAction>, IDataModelLoad<IUserCustomAction>, IDataModelUpdate, IDataModelDelete, IQueryableDataModel
    {
        /// <summary>
        /// Gets or sets the unique identifier of the associated client side component.
        /// </summary>
        public Guid ClientSideComponentId { get; set; }

        /// <summary>
        /// Gets or sets the JSON formatted properties of the associated client side component.
        /// </summary>
        public string ClientSideComponentProperties { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies an implementation specific XML fragment that determines user interface properties of the custom action.
        /// </summary>
        public string CommandUIExtension { get; set; }

        /// <summary>
        /// Gets or sets the description of the custom action.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies an implementation-specific value that determines the position of the custom action in the page.
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// This property is only used when an application customizer is created and preallocated placeholders are specified.  
        /// It is optional. If non-empty, the string must contain a JSON object with one or two properties for the height 
        /// amount needed to be preallocated.
        /// </summary>
        public string HostProperties { get; set; }

        /// <summary>
        /// Gets a value that specifies the identifier of the custom action.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets or sets the URL of the image associated with the custom action.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the location of the custom action.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the name of the custom action.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value that specifies the identifier of the object associated with the custom action.
        /// </summary>
        public string RegistrationId { get; set; }

        /// <summary>
        /// Gets or sets the value that specifies the type of object associated with the custom action.
        /// </summary>
        public UserCustomActionRegistrationType RegistrationType { get; set; }

        /// <summary>
        /// Gets a value that specifies the scope of the custom action.
        /// </summary>
        public UserCustomActionScope Scope { get; }

        /// <summary>
        /// Gets or sets the value that specifies the ECMAScript to be executed when the custom action is performed.
        /// </summary>
        public string ScriptBlock { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the URI of a file which contains the ECMAScript to execute on the page.
        /// </summary>
        public string ScriptSrc { get; set; }

        /// <summary>
        /// Gets or sets the value that specifies an implementation-specific value that determines the order of the custom action that appears on the page.
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// Gets or sets the display title of the custom action.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the URL, URI, or ECMAScript (JScript, JavaScript) function associated with the action.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Returns the version of the user custom action.
        /// </summary>
        public string VersionOfUserCustomAction { get; }

        /// <summary>
        /// Get or sets the base permissions of the action.
        /// </summary>
        public IBasePermissions Rights { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IUserResource DescriptionResource { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IUserResource TitleResource { get; }
    }
}
