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
        public string Id { get; set; }

        /// <summary>
        /// Name of the Office 365 Group
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Url of the SharePoint site connected to this Office 365 group
        /// </summary>
        public Uri WebUrl { get; set; }

        /// <summary>
        /// Email address of this Office 365 group
        /// </summary>
        public string Mail { get; set; }
    }
}
