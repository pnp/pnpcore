using PnP.Core.Provisioning.Model;

namespace PnP.Core.Provisioning.Extensibility
{
    /// <summary>
    /// Pre-processes a provisioning template or hierarchy before the engine runs.
    /// </summary>
    public interface IProvisioningTemplateProcessor
    {
        /// <summary>
        /// Pre-processes a provisioning template.
        /// </summary>
        /// <param name="template">The source template</param>
        /// <returns>The resulting template</returns>
        ProvisioningTemplate PreProcessTemplate(ProvisioningTemplate template);

        /// <summary>
        /// Pre-processes a provisioning hierarchy.
        /// </summary>
        /// <param name="hierarchy">The source hierarchy</param>
        /// <returns>The resulting hierarchy</returns>
        ProvisioningHierarchy PreProcessHierarchy(ProvisioningHierarchy hierarchy);
    }
}
