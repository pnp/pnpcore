using PnP.Core.Provisioning.Model;
using System.IO;

namespace PnP.Core.Provisioning.Providers
{
    /// <summary>
    /// Interface for basic capabilites that any ProvisioningHierarchy Formatter should provide/support
    /// </summary>
    public interface IProvisioningHierarchyFormatter
    {
        /// <summary>
        /// Method to format a ProvisioningHierarchy into a formatted provisioning file
        /// </summary>
        /// <param name="hierarchy">The input ProvisioningHierarchy</param>
        /// <returns>The output formatted hierarchy as a Stream</returns>
        Stream ToFormattedHierarchy(ProvisioningHierarchy hierarchy);

        /// <summary>
        /// Method to convert a formatted provisioning file into a ProvisioningHierarchy
        /// </summary>
        /// <param name="hierarchy">The input formatted provisioning file as a Stream</param>
        /// <returns>The output ProvisioningHierarchy</returns>
        ProvisioningHierarchy ToProvisioningHierarchy(Stream hierarchy);
    }
}
