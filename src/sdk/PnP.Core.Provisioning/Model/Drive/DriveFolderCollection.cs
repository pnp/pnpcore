namespace PnP.Core.Provisioning.Model.Drive
{
    /// <summary>
    /// Collection of DriveFolder items
    /// </summary>
    public partial class DriveFolderCollection : BaseProvisioningTemplateObjectCollection<DriveFolder>
    {
        /// <summary>
        /// Constructor for DriveFolderCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public DriveFolderCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
