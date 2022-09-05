using PnP.Core.Services.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// Public interface for the injectable service to create an PnPContext
    /// </summary>
    public interface IPnPContextFactory
    {
        /// <summary>
        /// Gets the EventHub instance, can be used to tap into events like request retry (due to throttling)
        /// </summary>
        public EventHub EventHub { get; }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided URL and Authentication Provider instance
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public PnPContext Create(Uri url, IAuthenticationProvider authenticationProvider, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided URL and Authentication Provider instance
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public PnPContext Create(Uri url, IAuthenticationProvider authenticationProvider, CancellationToken cancellationToken, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided URL and Authentication Provider instance
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public Task<PnPContext> CreateAsync(Uri url, IAuthenticationProvider authenticationProvider, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided URL and Authentication Provider instance
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public Task<PnPContext> CreateAsync(Uri url, IAuthenticationProvider authenticationProvider, CancellationToken cancellationToken, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided URL and using the default Authentication Provider
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public PnPContext Create(Uri url, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided URL and using the default Authentication Provider
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public PnPContext Create(Uri url, CancellationToken cancellationToken, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided URL and using the default Authentication Provider
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public Task<PnPContext> CreateAsync(Uri url, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided URL and using the default Authentication Provider
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public Task<PnPContext> CreateAsync(Uri url, CancellationToken cancellationToken, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and Authentication Provider instance
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public PnPContext Create(Guid groupId, IAuthenticationProvider authenticationProvider, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and Authentication Provider instance
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public PnPContext Create(Guid groupId, IAuthenticationProvider authenticationProvider, CancellationToken cancellationToken, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and Authentication Provider instance
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public Task<PnPContext> CreateAsync(Guid groupId, IAuthenticationProvider authenticationProvider, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and Authentication Provider instance
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public Task<PnPContext> CreateAsync(Guid groupId, IAuthenticationProvider authenticationProvider, CancellationToken cancellationToken, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and using the default Authentication Provider
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public PnPContext Create(Guid groupId, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and using the default Authentication Provider
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public PnPContext Create(Guid groupId, CancellationToken cancellationToken, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and using the default Authentication Provider
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public Task<PnPContext> CreateAsync(Guid groupId, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and using the default Authentication Provider
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public Task<PnPContext> CreateAsync(Guid groupId, CancellationToken cancellationToken, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name and on a custom initialization function for the IAuthenticationProvider reference instance
        /// </summary>
        /// <param name="name">The name of the configuration to use</param>
        /// <param name="initializeAuthenticationProvider">The function to initialize the context</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public PnPContext Create(string name, Action<IAuthenticationProvider> initializeAuthenticationProvider, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name and on a custom initialization function for the IAuthenticationProvider reference instance
        /// </summary>
        /// <param name="name">The name of the configuration to use</param>
        /// <param name="initializeAuthenticationProvider">The function to initialize the context</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public PnPContext Create(string name, Action<IAuthenticationProvider> initializeAuthenticationProvider, CancellationToken cancellationToken, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name and on a custom initialization function for the IAuthenticationProvider reference instance
        /// </summary>
        /// <param name="name">The name of the configuration to use</param>
        /// <param name="initializeAuthenticationProvider">The function to initialize the context</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public Task<PnPContext> CreateAsync(string name, Action<IAuthenticationProvider> initializeAuthenticationProvider, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name and on a custom initialization function for the IAuthenticationProvider reference instance
        /// </summary>
        /// <param name="name">The name of the configuration to use</param>
        /// <param name="initializeAuthenticationProvider">The function to initialize the context</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public Task<PnPContext> CreateAsync(string name, Action<IAuthenticationProvider> initializeAuthenticationProvider, CancellationToken cancellationToken, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the configuration to use</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public PnPContext Create(string name, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the configuration to use</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public PnPContext Create(string name, CancellationToken cancellationToken, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the configuration to use</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public Task<PnPContext> CreateAsync(string name, PnPContextOptions options = null);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the configuration to use</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public Task<PnPContext> CreateAsync(string name, CancellationToken cancellationToken, PnPContextOptions options = null);
    }
}
