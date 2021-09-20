using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Test.Common.Services
{
    public interface IPnPTestContextFactory : IPnPContextFactory
    {
        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name, without initialization
        /// </summary>
        /// <param name="name">The name of the configuration to use</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public Task<PnPContext> CreateWithoutInitializationAsync(string name);

        /// <summary>
        /// Creates a live instance of PnPContext based on a URI
        /// </summary>
        /// <param name="url">Url for the context</param>
        /// <param name="authenticationProvider">Authentication provider to use</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public Task<PnPContext> CreateLiveAsync(Uri url, IAuthenticationProvider authenticationProvider);
    }
}
