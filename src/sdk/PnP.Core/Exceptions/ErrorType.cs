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
        OfflineDataError
    }
}
