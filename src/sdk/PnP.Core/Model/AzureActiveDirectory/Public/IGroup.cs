using System;

namespace PnP.Core.Model.AzureActiveDirectory
{
    /// <summary>
    /// An Office 365 Group
    /// </summary>
    public interface IGroup : IDataModel<IGroup>
    {
        /// <summary>
        /// Id of the Office 365 Group
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Name of the Office 365 Group
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Description of the Office 365 Group
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Url of the SharePoint site connected to this Office 365 group
        /// </summary>
        public Uri WebUrl { get; }

        /// <summary>
        /// Is this group mail enabled
        /// </summary>
        public bool MailEnabled { get; set; }

        /// <summary>
        /// Email address of this Office 365 group
        /// </summary>
        public string Mail { get; }

        /// <summary>
        /// Mail nickname of this Office 365 group
        /// </summary>
        public string MailNickname { get; }

        /// <summary>
        /// Classification of this group
        /// </summary>
        public string Classification { get; set; }

        /// <summary>
        /// When was this group created
        /// </summary>
        public DateTimeOffset CreatedDateTime { get; }

        /// <summary>
        /// Visibility of this group
        /// </summary>
        public GroupVisibility Visibility { get; set; }

    }
}
