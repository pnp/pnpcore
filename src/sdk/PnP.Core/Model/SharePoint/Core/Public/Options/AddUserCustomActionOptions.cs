using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options to specify when adding User Custom Action
    /// </summary>
    public class AddUserCustomActionOptions
    {
        /// <summary>
        /// Gets or sets the unique identifier of the associated client side component.
        /// </summary>
        public Guid? ClientSideComponentId { get; set; }

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

        //TODO: Is not in CSOM, double-check what it is used for and format
        /// <summary>
        /// To update...
        /// </summary>
        public string HostProperties { get; set; }

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
        public UserCustomActionRegistrationType? RegistrationType { get; set; }

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
        public int? Sequence { get; set; }

        /// <summary>
        /// Gets or sets the display title of the custom action.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the URL, URI, or ECMAScript (JScript, JavaScript) function associated with the action.
        /// </summary>
        public string Url { get; set; }
    }
}
