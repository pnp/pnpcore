namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of Folder objects
    /// </summary>
    public partial class FolderCollection : BaseProvisioningTemplateObjectCollection<Folder>
    {
        /// <summary>
        /// Constructor for Folder class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public FolderCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
