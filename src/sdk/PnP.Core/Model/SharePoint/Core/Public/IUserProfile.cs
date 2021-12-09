using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// An entry point for the user profile APIs
    /// </summary>
    [ConcreteType(typeof(UserProfile))]
    public interface IUserProfile : IDataModel<IUserProfile>, IDataModelGet<IUserProfile>, IDataModelLoad<IUserProfile>
    {
        /// <summary>
        /// The link to edit the current user's profile
        /// </summary>
        string EditProfileLink { get; }

        /// <summary>
        /// A Boolean value that indicates whether the current user's People I'm Following list is public.
        /// </summary>
        bool IsMyPeopleListPublic { get; }

        /// <summary>
        /// Gets user properties for the current user.
        /// </summary>
        /// <param name="selectors">Specific properties to return</param>
        /// <returns>An instance of <see cref="IPersonProperties"/></returns>
        Task<IPersonProperties> GetMyPropertiesAsync(params Expression<Func<IPersonProperties, object>>[] selectors);

        /// <summary>
        /// Gets user properties for the current user.
        /// </summary>
        /// <param name="selectors">Specific properties to return</param>
        /// <returns>An instance of <see cref="IPersonProperties"/></returns>
        IPersonProperties GetMyProperties(params Expression<Func<IPersonProperties, object>>[] selectors);

        /// <summary>
        /// Gets user properties for the specified user.
        /// </summary>
        /// <param name="accountName">The account name, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <param name="selectors">Specific properties to return</param>
        /// <returns>An instance of <see cref="IPersonProperties"/></returns>
        Task<IPersonProperties> GetPropertiesForAsync(string accountName, params Expression<Func<IPersonProperties, object>>[] selectors);

        /// <summary>
        /// Gets user properties for the specified user.
        /// </summary>
        /// <param name="accountName">The account name, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <param name="selectors">Specific properties to return</param>
        /// <returns>An instance of <see cref="IPersonProperties"/></returns>
        IPersonProperties GetPropertiesFor(string accountName, params Expression<Func<IPersonProperties, object>>[] selectors);

        /// <summary>
        /// Gets the specified user profile property for the specified user.
        /// </summary>
        /// <param name="accountName">The account name, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <param name="propertyName">The case-sensitive name of the property to get.</param>
        /// <returns>The specified profile property for the specified user.</returns>
        Task<string> GetPropertyForAsync(string accountName, string propertyName);

        /// <summary>
        /// Gets the specified user profile property for the specified user.
        /// </summary>
        /// <param name="accountName">The account name, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <param name="propertyName">The case-sensitive name of the property to get.</param>
        /// <returns>The specified profile property for the specified user.</returns>
        string GetPropertyFor(string accountName, string propertyName);

        /// <summary>
        /// Sets the user's profile property.
        /// </summary>
        /// <param name="accountName">The account name, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <param name="propertyName">The case-sensitive name of the property to get.</param>
        /// <param name="propertyValue">The property value</param>
        Task SetSingleValueProfilePropertyAsync(string accountName, string propertyName, string propertyValue);

        /// <summary>
        /// Sets the user's profile property.
        /// </summary>
        /// <param name="accountName">The account name, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <param name="propertyName">The case-sensitive name of the property to get.</param>
        /// <param name="propertyValue">The property value</param>
        void SetSingleValueProfileProperty(string accountName, string propertyName, string propertyValue);

        /// <summary>
        /// Sets the user's multi value profile property.
        /// </summary>
        /// <param name="accountName">The account name, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <param name="propertyName">The case-sensitive name of the property to get.</param>
        /// <param name="propertyValues">The property values</param>
        Task SetMultiValuedProfilePropertyAsync(string accountName, string propertyName, IList<string> propertyValues);

        /// <summary>
        /// Sets the user's multi value profile property.
        /// </summary>
        /// <param name="accountName">The account name, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <param name="propertyName">The case-sensitive name of the property to get.</param>
        /// <param name="propertyValues">The property values</param>
        void SetMultiValuedProfileProperty(string accountName, string propertyName, IList<string> propertyValues);

        /// <summary>
        /// Uploads and sets the user profile picture. Users can upload a picture to their own profile only.
        /// </summary>
        /// <param name="fileBytes">A byte array representing the file</param>
        Task SetMyProfilePictureAsync(byte[] fileBytes);

        /// <summary>
        /// Uploads and sets the user profile picture. Users can upload a picture to their own profile only.
        /// </summary>
        /// <param name="fileBytes">A byte array representing the file</param>
        void SetMyProfilePicture(byte[] fileBytes);

        /// <summary>
        /// Gets the user's onedrive max quota
        /// </summary>
        /// <param name="accountName">The account name, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <returns>The max quota value</returns>
        Task<long> GetUserOneDriveQuotaMaxAsync(string accountName);

        /// <summary>
        /// Gets the user's onedrive max quota
        /// </summary>
        /// <param name="accountName">The account name, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <returns>The max quota value</returns>
        long GetUserOneDriveQuotaMax(string accountName);

        /// <summary>
        /// Resets the user's onedrive quota to the default value
        /// </summary>
        /// <param name="accountName">The account name, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <returns>The string outcome</returns>
        Task<string> ResetUserOneDriveQuotaToDefaultAsync(string accountName);

        /// <summary>
        /// Resets the user's onedrive quota to the default value
        /// </summary>
        /// <param name="accountName">The account name, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <returns>The string outcome</returns>
        string ResetUserOneDriveQuotaToDefault(string accountName);

        /// <summary>
        /// Sets the user's onedrive quota
        /// </summary>
        /// <param name="accountName">The account name, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <param name="newQuota">New quota value</param>
        /// <param name="newQuotaWarning">Quota warning value</param>
        /// <returns>The string outcome</returns>
        Task<string> SetUserOneDriveQuotaAsync(string accountName, long newQuota, long newQuotaWarning);

        /// <summary>
        /// Sets the user's onedrive quota
        /// </summary>
        /// <param name="accountName">The account name, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <param name="newQuota">New quota value</param>
        /// <param name="newQuotaWarning">Quota warning value</param>
        /// <returns>The string outcome</returns>
        string SetUserOneDriveQuota(string accountName, long newQuota, long newQuotaWarning);
    }
}
