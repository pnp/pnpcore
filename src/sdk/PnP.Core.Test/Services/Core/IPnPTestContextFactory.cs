using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Test.Services
{
    public interface IPnPTestContextFactory : IPnPContextFactory
    {
        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name, without initialization
        /// </summary>
        /// <param name="name">The name of the configuration to use</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public Task<PnPContext> CreateWithoutInitializationAsync(string name);
    }
}
