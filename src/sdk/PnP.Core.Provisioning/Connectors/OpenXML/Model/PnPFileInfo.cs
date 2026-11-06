using System;

namespace PnP.Core.Provisioning.Connectors.OpenXML.Model
{
    /// <summary>
    /// File descriptor for every single file in the PnP OpenXML file
    /// </summary>
    [Serializable]
    public class PnPFileInfo
    {
        /// <summary>
        /// The Internal Name of the file in the PnP OpenXML file
        /// </summary>
        public string InternalName { get; set; }

        /// <summary>
        /// The Original Name of the file in the source template
        /// </summary>
        public string OriginalName { get; set; }

        /// <summary>
        /// The name of the folder within the PnP OpenXML file
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// The binary content of the file
        /// </summary>
        public Byte[] Content { get; set; }
    }
}
