namespace PnP.Core
{
    /// <summary>
    /// List of possible errors that can be thrown 
    /// </summary>
    public enum ErrorType
    {
        // Service errors
        GraphServiceError,
        SharePointRestServiceError,

        // Authentication errors
        AzureADError,

        // Configuration errors
        MissingAddApiHandler,
        ModelMetadataIncorrect,
        GraphBetaNotAllowed,
        Unsupported,
        CollectionNotLoaded,
        PropertyNotLoaded,
        InstanceWasDeleted,
        LinqError,
        ConfigurationError,

        // Testing
        OfflineDataError,

        // Teams
        TeamsAsyncOperationError,

        // Http
        TooManyRetries,
        TooManyBatchRetries
    }
}
