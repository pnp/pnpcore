namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// A status code of the following item, a property of <see cref="ISocialActor"/>
    /// </summary>
    public enum SocialStatusCode
    {
        /**
       * The operation completed successfully
       */
        OK,
        /**
       * The request is invalid.
       */
        InvalidRequest,
        /**
       *  The current user is not authorized to perform the operation.
       */
        AccessDenied,
        /**
       * The target of the operation was not found.
       */
        ItemNotFound,
        /**
       * The operation is invalid for the target's current state.
       */
        InvalidOperation,
        /**
       * The operation completed without modifying the target.
       */
        ItemNotModified,
        /**
       * The operation failed because an internal error occurred.
       */
        InternalError,
        /**
       * The operation failed because the server could not access the distributed cache.
       */
        CacheReadError,
        /**
       * The operation succeeded but the server could not update the distributed cache.
       */
        CacheUpdateError,
        /**
       * No personal site exists for the current user, and no further information is available.
       */
        PersonalSiteNotFound,
        /**
       * No personal site exists for the current user, and a previous attempt to create one failed.
       */
        FailedToCreatePersonalSite,
        /**
       * No personal site exists for the current user, and a previous attempt to create one was not authorized.
       */
        NotAuthorizedToCreatePersonalSite,
        /**
       * No personal site exists for the current user, and no attempt should be made to create one.
       */
        CannotCreatePersonalSite,
        /**
       * The operation was rejected because an internal limit had been reached.
       */
        LimitReached,
        /**
       * The operation failed because an error occurred during the processing of the specified attachment.
       */
        AttachmentError,
        /**
       * The operation succeeded with recoverable errors; the returned data is incomplete.
       */
        PartialData,
        /**
       * A required SharePoint feature is not enabled.
       */
        FeatureDisabled,
        /**
       * The site's storage quota has been exceeded.
       */
        StorageQuotaExceeded,
        /**
       * The operation failed because the server could not access the database.
       */
        DatabaseError,
    }
}
