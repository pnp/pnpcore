using System;

namespace PnP.Core.Services
{
    /// <summary>
    /// Public type to define the Fake Authentication Provider configuration
    /// </summary>
    public class FakeAuthenticationProviderConfiguration : IAuthenticationProviderConfiguration
    {
        /// <summary>
        /// A fake setting for the Fake Authentication Provider
        /// </summary>
        public string FakeSetting { get; set; }

        /// <summary>
        /// The Name of the configuration
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Defines the type of the IAuthenticationProvider to create
        /// </summary>
        public Type AuthenticationProviderType => typeof(FakeAuthenticationProvider);
    }
}
