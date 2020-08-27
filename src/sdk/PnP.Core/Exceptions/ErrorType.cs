namespace PnP.Core
{
    /// <summary>
    /// List of possible errors that can be thrown 
    /// </summary>
    public enum ErrorType
    {
        // Service errors
        
        /// <summary>
        /// Graph service request returned an error
        /// </summary>
        GraphServiceError,

        /// <summary>
        /// SharePoint REST request returned an error
        /// </summary>
        SharePointRestServiceError,

        /// <summary>
        /// CSOM request returned an error
        /// </summary>
        CsomServiceError,

        // Authentication errors

        /// <summary>
        /// Authentication call to Azure AD failed
        /// </summary>
        AzureADError,

        // Configuration errors

        /// <summary>
        /// Adding an item to Microsoft 365 using 'AddAsync' requires that the added model class has an AddApiHandler configured. See contribution docs to learn more
        /// </summary>
        MissingAddApiHandler,

        /// <summary>
        /// There's incomplete model metadata configured. See contribution docs to learn more.
        /// </summary>
        ModelMetadataIncorrect,

        /// <summary>
        /// Making a Graph Beta call is not allowed. See usage docs to learn more
        /// </summary>
        GraphBetaNotAllowed,

        /// <summary>
        /// Unsupported action
        /// </summary>
        Unsupported,

        /// <summary>
        /// Collection needs to be loaded once before you can use the paging methods for paged data retrieval
        /// </summary>
        CollectionNotLoaded,

        /// <summary>
        /// The property you want to use was not yet loaded, first request it before using it
        /// </summary>
        PropertyNotLoaded,

        /// <summary>
        /// This model instance was deleted, you can't use it anymore
        /// </summary>
        InstanceWasDeleted,

        /// <summary>
        /// Something went wrong with issuing a linq query
        /// </summary>
        LinqError,

        /// <summary>
        /// There's an issue in the provided configuration data
        /// </summary>
        ConfigurationError,

        /// <summary>
        /// Invalid parameters are sent it for a request
        /// </summary>
        InvalidParameters,

        /// <summary>
        /// The API call still contains unresolved tokens
        /// </summary>
        UnresolvedTokens,

        // Testing

        /// <summary>
        /// There's an issue with the available offline test data
        /// </summary>
        OfflineDataError,

        // Teams

        /// <summary>
        /// Something went wrong when doing an Teams async operation
        /// </summary>
        TeamsAsyncOperationError,

        // Http

        /// <summary>
        /// Too many retries of an http request happened
        /// </summary>
        TooManyRetries,

        /// <summary>
        /// Too many retries of a request in a Graph batch happened
        /// </summary>
        TooManyBatchRetries
    }
}
