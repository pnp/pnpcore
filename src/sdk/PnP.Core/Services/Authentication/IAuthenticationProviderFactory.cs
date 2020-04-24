namespace PnP.Core.Services
{
    /// <summary>
    /// Public interface for the injectable service to create instances of the interface IAuthenticationProvider
    /// </summary>
    public interface IAuthenticationProviderFactory
    {
        /// <summary>
        /// Creates a new instance of IAuthenticationProvider based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the IAuthenticationProvider configuration to use</param>
        /// <returns>An object that implements IAuthenticationProvider based on the provided configuration name</returns>
        public IAuthenticationProvider Create(string name);

        /// <summary>
        /// Creates the default instance of IAuthenticationProvider based on the configuration
        /// </summary>
        /// <returns>An object that implements IAuthenticationProvider based on the configuration</returns>
        public IAuthenticationProvider CreateDefault();
    }
}
