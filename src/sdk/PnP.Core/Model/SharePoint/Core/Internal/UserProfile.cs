using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.UserProfiles.PeopleManager", Uri = Uri)]
    internal sealed class UserProfile : SocialBase<IUserProfile>, IUserProfile
    {
        private const string Uri = "_api/SP.UserProfiles.PeopleManager";

        public string EditProfileLink { get => GetValue<string>(); set => SetValue(value); }
        public bool IsMyPeopleListPublic { get => GetValue<bool>(); set => SetValue(value); }

        [KeyProperty(nameof(EditProfileLink))]
        public override object Key { get => EditProfileLink; }

        public IPersonProperties GetMyProperties(params Expression<Func<IPersonProperties, object>>[] selectors)
        {
            return GetMyPropertiesAsync(selectors).GetAwaiter().GetResult();
        }

        public async Task<IPersonProperties> GetMyPropertiesAsync(params Expression<Func<IPersonProperties, object>>[] selectors)
        {
            return await GetGenericPeopleManagerResultAsync($"{Uri}/getmyproperties", selectors).ConfigureAwait(false);
        }

        public IPersonProperties GetPropertiesFor(string accountName, params Expression<Func<IPersonProperties, object>>[] selectors)
        {
            return GetPropertiesForAsync(accountName, selectors).GetAwaiter().GetResult();
        }

        public async Task<IPersonProperties> GetPropertiesForAsync(string accountName, params Expression<Func<IPersonProperties, object>>[] selectors)
        {
            var baseUrl = $"{Uri}/getpropertiesfor(accountname='{HttpUtility.UrlEncode(accountName)}')";

            return await GetGenericPeopleManagerResultAsync(baseUrl, selectors).ConfigureAwait(false);
        }

        public string GetPropertyFor(string accountName, string propertyName)
        {
            return GetPropertyForAsync(accountName, propertyName).GetAwaiter().GetResult();
        }

        public async Task<string> GetPropertyForAsync(string accountName, string propertyName)
        {
            var baseUrl = $"{Uri}/GetUserProfilePropertyFor(accountname='{HttpUtility.UrlEncode(accountName)}',propertyname='{HttpUtility.UrlEncode(propertyName)}')";

            return await GetSingleResult(baseUrl).ConfigureAwait(false);
        }

        public long GetUserOneDriveQuotaMax(string accountName)
        {
            return GetUserOneDriveQuotaMaxAsync(accountName).GetAwaiter().GetResult();
        }

        public async Task<long> GetUserOneDriveQuotaMaxAsync(string accountName)
        {
            var baseUrl = $"{Uri}/GetUserOneDriveQuotaMax(accountname='{HttpUtility.UrlEncode(accountName)}')";

            return long.Parse(await GetSingleResult(baseUrl).ConfigureAwait(false));
        }

        public string ResetUserOneDriveQuotaToDefault(string accountName)
        {
            return ResetUserOneDriveQuotaToDefaultAsync(accountName).GetAwaiter().GetResult();
        }

        public async Task<string> ResetUserOneDriveQuotaToDefaultAsync(string accountName)
        {
            var baseUrl = $"{Uri}/ResetUserOneDriveQuotaToDefault(accountname='{HttpUtility.UrlEncode(accountName)}')";

            return await GetSingleResult(baseUrl).ConfigureAwait(false);
        }

        public void SetMultiValuedProfileProperty(string accountName, string propertyName, IList<string> propertyValues)
        {
            SetMultiValuedProfilePropertyAsync(accountName, propertyName, propertyValues).GetAwaiter().GetResult();
        }

        public async Task SetMultiValuedProfilePropertyAsync(string accountName, string propertyName, IList<string> propertyValues)
        {
            var baseUrl = $"{Uri}/SetMultiValuedProfileProperty";
            var body = new
            {
                accountName,
                propertyName,
                propertyValues
            };

            var apiCall = new ApiCall(baseUrl, ApiType.SPORest)
            {
                JsonBody = JsonSerializer.Serialize(body)
            };

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void SetMyProfilePicture(byte[] fileBytes)
        {
            SetMyProfilePictureAsync(fileBytes).GetAwaiter().GetResult();
        }

        public async Task SetMyProfilePictureAsync(byte[] fileBytes)
        {
            var baseUrl = $"{Uri}/SetMyProfilePicture";

            var apiCall = new ApiCall(baseUrl, ApiType.SPORest)
            {
                Interactive = true,
                Content = new ByteArrayContent(fileBytes)
            };

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void SetSingleValueProfileProperty(string accountName, string propertyName, string propertyValue)
        {
            SetSingleValueProfilePropertyAsync(accountName, propertyName, propertyValue).GetAwaiter().GetResult();
        }

        public async Task SetSingleValueProfilePropertyAsync(string accountName, string propertyName, string propertyValue)
        {
            var baseUrl = $"{Uri}/SetSingleValueProfileProperty(accountname='{HttpUtility.UrlEncode(accountName)}',propertyName='{HttpUtility.UrlEncode(propertyName)}',propertyValue='{propertyValue}')";
            var apiCall = new ApiCall(baseUrl, ApiType.SPORest);

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public async Task<string> SetUserOneDriveQuotaAsync(string accountName, long newQuota, long newQuotaWarning)
        {
            var baseUrl = $"{Uri}/SetUserOneDriveQuota(accountname='{HttpUtility.UrlEncode(accountName)}',newQuota={newQuota},newQuotaWarning={newQuotaWarning})";

            return await GetSingleResult(baseUrl).ConfigureAwait(false);
        }

        public string SetUserOneDriveQuota(string accountName, long newQuota, long newQuotaWarning)
        {
            return SetUserOneDriveQuotaAsync(accountName, newQuota, newQuotaWarning).GetAwaiter().GetResult();
        }
    }
}
