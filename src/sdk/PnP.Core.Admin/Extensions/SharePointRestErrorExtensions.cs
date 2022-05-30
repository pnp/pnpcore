namespace PnP.Core.Admin.Extensions
{
    /// <summary>
    /// Extends the <see cref="SharePointRestError"/> with additional functionality
    /// </summary>
    public static class SharePointRestErrorExtensions
    {
        /// <summary>
        /// Extends a <see cref="SharePointRestServiceException"/> with if a site is accesable
        /// </summary>
        /// <param name="ex"><see cref="SharePointRestServiceException"/> to extend</param>
        /// <returns>An <see cref="bool"/> if a site is accesable</returns>
        public static bool IsCannotGetSiteException(this SharePointRestServiceException ex)
        {
            var error = ex.Error as SharePointRestError;
            if (error.ServerErrorCode == -1)
                return true;

            return false;
        }

        /// <summary>
        /// Extends a <see cref="SharePointRestServiceException"/> with if a context has access to the site
        /// </summary>
        /// <param name="ex"><see cref="SharePointRestServiceException"/> to extend</param>
        /// <returns>An <see cref="bool"/> bool if has access</returns>
        public static bool IsUnableToAccessSiteException(this SharePointRestServiceException ex)
        {
            var error = ex.Error as SharePointRestError;
            if (error.ServerErrorCode == -2147024809)
                return true;

            return false;
        }
    }
}
