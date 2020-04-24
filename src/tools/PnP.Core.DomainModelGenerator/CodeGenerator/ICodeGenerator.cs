using System.Threading.Tasks;

namespace PnP.M365.DomainModelGenerator
{
    /// <summary>
    /// Abstract interface for the Code Generator service
    /// </summary>
    internal interface ICodeGenerator
    {
        /// <summary>
        /// Processes the in memory Model to generate the code
        /// </summary>
        /// <returns></returns>
        Task ProcessAsync(Model model);
    }
}
