namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of Feature objects
    /// </summary>
    public partial class FeatureCollection : BaseProvisioningTemplateObjectCollection<Feature>
    {
        /// <summary>
        /// Constructor for FeaturCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public FeatureCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
