using Microsoft.Extensions.Logging;
using PnP.Core.Model.Me;
using PnP.Core.Model.Security;
using PnP.Core.Model.SharePoint;
using PnP.Core.Model.Teams;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// PnPContext interface to support mocking (that's the only reason)
    /// </summary>
    public interface IPnPContext : IDisposable
    {
        /// <summary>
        /// Entry point for the Web Object
        /// </summary>
        IWeb Web { get; }

        /// <summary>
        /// Entry point for the Site Object
        /// </summary>
        ISite Site { get; }

        /// <summary>
        /// Entry point for the Team Object
        /// </summary>
        ITeam Team { get; }

        /// <summary>
        /// Entry point for the Group Object
        /// </summary>
        IGraphGroup Group { get; }

        /// <summary>
        /// Entry point for the TermStore Object
        /// </summary>
        ITermStore TermStore { get; }

        /// <summary>
        /// Entry point for the Social Object
        /// </summary>
        ISocial Social { get; }

        /// <summary>
        /// Entry point for the Me Object
        /// </summary>
        IMe Me { get; }

        /// <summary>
        /// Entry point for the ContentTypeHub Object
        /// </summary>
        IContentTypeHub ContentTypeHub { get; }

        /// <summary>
        /// Uri of the SharePoint site we're working against
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// Connected logger
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        /// Connected authentication provider
        /// </summary>
        IAuthenticationProvider AuthenticationProvider { get; }

        /// <summary>
        /// Connected SharePoint REST client
        /// </summary>
        SharePointRestClient RestClient { get; }

        /// <summary>
        /// Connected Microsoft Graph client
        /// </summary>
        MicrosoftGraphClient GraphClient { get; }

        /// <summary>
        /// Returns the used Microsoft 365 cloud environment
        /// </summary>
        Microsoft365Environment? Environment { get; }

        /// <summary>
        /// Returns the Microsoft Graph authority (e.g. graph.microsoft.com) to use when <see cref="Environment"/> is set to <see cref="Microsoft365Environment.Custom"/>
        /// </summary>
        string MicrosoftGraphAuthority { get; }

        /// <summary>
        /// Returns the Azure AD Login authority (e.g. login.microsoftonline.com) to use when <see cref="Environment"/> is set to <see cref="Microsoft365Environment.Custom"/>
        /// </summary>
        string AzureADLoginAuthority { get; }

        /// <summary>
        /// Collection for custom properties that you want to attach to a <see cref="PnPContext"/>
        /// </summary>
        IDictionary<string, object> Properties { get; }

        /// <summary>
        /// Controls whether the library will try to use Microsoft Graph over REST whenever that's defined in the model
        /// </summary>
        bool GraphFirst { get; set; } 

        /// <summary>
        /// If true than all requests to Microsoft Graph use the beta endpoint
        /// </summary>
        bool GraphAlwaysUseBeta { get; set; }

        /// <summary>
        /// If true than the Graph beta endpoint is used when there's no other option, default approach stays using the v1 endpoint
        /// </summary>
        bool GraphCanUseBeta { get; set; }

        /// <summary>
        /// Are there pending requests to execute (in the case of batching)
        /// </summary>
        bool HasPendingRequests { get; }

        /// <summary>
        /// Current batch, used for implicit batching
        /// </summary>
        Batch CurrentBatch { get; }

        /// <summary>
        /// Creates a new batch
        /// </summary>
        /// <returns>A new <see cref="Batch"/> instance for batching.</returns>
        Batch NewBatch();

        /// <summary>
        /// Gets an ongoing Graph long-running operation.
        /// </summary>
        /// <param name="location">The location of the operation</param>
        /// <returns>An <see cref="ILongRunningOperation"/> associated with the location</returns>
        ILongRunningOperation GetLongRunningOperation(string location);

        /// <summary>
        /// Method to execute the current batch
        /// </summary>
        /// <param name="throwOnError">Throw an exception on the first encountered error in the batch</param>
        /// <returns>The asynchronous task that will be executed</returns>
        Task<List<BatchResult>> ExecuteAsync(bool throwOnError = true);

        /// <summary>
        /// Method to execute a given batch
        /// </summary>
        /// <param name="batch">Batch to execute</param>
        /// <param name="throwOnError">Throw an exception on the first encountered error in the batch</param>
        /// <returns>The asynchronous task that will be executed</returns>
        Task<List<BatchResult>> ExecuteAsync(Batch batch, bool throwOnError = true);

        /// <summary>
        /// Method to execute the current batch
        /// </summary>
        /// <param name="throwOnError">Throw an exception on the first encountered error in the batch</param>
        /// <returns>The asynchronous task that will be executed</returns>
        List<BatchResult> Execute(bool throwOnError = true);

        /// <summary>
        /// Method to execute a given batch
        /// </summary>
        /// <param name="batch">Batch to execute</param>
        /// <param name="throwOnError">Throw an exception on the first encountered error in the batch</param>
        /// <returns>The asynchronous task that will be executed</returns>
        List<BatchResult> Execute(Batch batch, bool throwOnError = true);

        /// <summary>
        /// Clones this context into a new context for the same SharePoint site
        /// </summary>
        /// <returns>New <see cref="PnPContext"/></returns>
        PnPContext Clone();

        /// <summary>
        /// Clones this context into a new context for the same SharePoint site
        /// </summary>
        /// <returns>New <see cref="PnPContext"/></returns>
        Task<PnPContext> CloneAsync();

        /// <summary>
        /// Clones this context for another SharePoint site provided as configuration
        /// </summary>
        /// <param name="name">The name of the SPOContext configuration to use</param>
        /// <returns>New <see cref="PnPContext"/> for the request config</returns>
        PnPContext Clone(string name);

        /// <summary>
        /// Clones this context for another SharePoint site provided as configuration
        /// </summary>
        /// <param name="name">The name of the SPOContext configuration to use</param>
        /// <returns>New <see cref="PnPContext"/> for the request config</returns>
        Task<PnPContext> CloneAsync(string name);

        /// <summary>
        /// Clones this context for another SharePoint site
        /// </summary>
        /// <param name="uri">Uri of the other SharePoint site</param>
        /// <returns>New <see cref="PnPContext"/></returns>
        PnPContext Clone(Uri uri);

        /// <summary>
        /// Clones this context for another SharePoint site
        /// </summary>
        /// <param name="uri">Uri of the other SharePoint site</param>
        /// <returns>New <see cref="PnPContext"/></returns>
        Task<PnPContext> CloneAsync(Uri uri);

        /// <summary>
        /// Clones this context for another SharePoint site
        /// </summary>
        /// <param name="groupId">Id of the other Microsoft 365 group to create a <see cref="PnPContext"/> for</param>
        /// <returns>New <see cref="PnPContext"/></returns>
        PnPContext Clone(Guid groupId);

        /// <summary>
        /// Clones this context for another SharePoint site
        /// </summary>
        /// <param name="groupId">Id of the other Microsoft 365 group to create a <see cref="PnPContext"/> for</param>
        /// <returns>New <see cref="PnPContext"/></returns>
        Task<PnPContext> CloneAsync(Guid groupId);
    }
}
