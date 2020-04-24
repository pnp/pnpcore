using System.Threading.Tasks;

namespace PnP.M365.DomainModelGenerator
{
    /// <summary>
    /// Abstract interface for the EDMX metadata processor
    /// </summary>
    internal interface IEdmxProcessor
    {
        /// <summary>
        /// Processes an EDMX file
        /// </summary>
        /// <returns>The entity model generated from the EDMX metadata files</returns>
        Task<Model> ProcessAsync();
    }
}
