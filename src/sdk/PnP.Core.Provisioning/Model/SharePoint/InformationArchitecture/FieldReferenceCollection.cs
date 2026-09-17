namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of Field Reference objects
    /// </summary>
    public partial class FieldReferenceCollection : BaseProvisioningTemplateObjectCollection<FieldReference>
    {
        /// <summary>
        /// Constructor for FieldReferenceCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public FieldReferenceCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
