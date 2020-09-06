using System;

namespace PnP.Core.Model.SharePoint
{
    // TODO: CSOM seems to expose more properties, double-check if those properties are not exposed in REST
    // https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-server/mt683918(v=office.15)
    /// <summary>
    /// Public interface to define a FileVersionEvent object
    /// </summary>
    [ConcreteType(typeof(FileVersionEvent))]
    public interface IFileVersionEvent : IComplexType
    {
        /// <summary>
        /// Gets the editor of this version of the file.
        /// </summary>
        public string Editor { get; }

        /// <summary>
        /// Gets the e-mail address of the editor of this version of the file.
        /// </summary>
        public string EditorEmail { get; set; }

        /// <summary>
        /// Gets the time of this version of the file.
        /// </summary>
        public DateTime Time { get; set; }
    }
}
