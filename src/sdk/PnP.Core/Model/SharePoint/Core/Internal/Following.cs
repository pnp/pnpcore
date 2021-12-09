using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.Social.SocialRestFollowingManager", Uri = Uri)]
    internal class Following : SocialBase<IFollowing>, IFollowing
    {
        private const string Uri = "_api/social.following";

        public IList<IPersonProperties> GetFollowersFor(string accountName, params Expression<Func<IPersonProperties, object>>[] selectors)
        {
            return GetFollowersForAsync(accountName, selectors).GetAwaiter().GetResult();
        }

        public async Task<IList<IPersonProperties>> GetFollowersForAsync(string accountName, params Expression<Func<IPersonProperties, object>>[] selectors)
        {
            var baseUrl = $"_api/SP.UserProfiles.PeopleManager/getfollowersfor(accountName='{HttpUtility.UrlEncode(accountName)}')";

            return await GetGenericPeopleManagerResultsAsync(baseUrl, selectors).ConfigureAwait(false);
        }

        public IFollowingInfo GetFollowingInfo()
        {
            return GetFollowingInfoAsync().GetAwaiter().GetResult();
        }

        public async Task<IFollowingInfo> GetFollowingInfoAsync()
        {
            var apiCall = new ApiCall($"{Uri}/my", ApiType.SPORest)
            {
                Interactive = true,
            };

            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);
            var document = JsonSerializer.Deserialize<JsonElement>(response.Json);

            var modelJson = document.Get("d");

            var entityInfo = EntityManager.Instance.GetStaticClassInfo(typeof(FollowingInfo));
            var socialInfo = new FollowingInfo();
            await JsonMappingHelper.FromJson(socialInfo, entityInfo, new ApiResponse(apiCall, modelJson.Value, Guid.Empty)).ConfigureAwait(false);

            var socialActorNode = modelJson.Value.Get(nameof(socialInfo.SocialActor));
            if (socialActorNode != null)
            {
                var socialActor = new SocialActor();
                socialInfo.SocialActor = socialActor;
                entityInfo = EntityManager.Instance.GetStaticClassInfo(typeof(SocialActor));
                await JsonMappingHelper.FromJson(socialActor, entityInfo, new ApiResponse(apiCall, socialActorNode.Value, Guid.Empty)).ConfigureAwait(false);
            }

            return socialInfo;
        }

        public IList<IPersonProperties> GetPeopleFollowedBy(string accountName, params Expression<Func<IPersonProperties, object>>[] selectors)
        {
            return GetPeopleFollowedByAsync(accountName, selectors).GetAwaiter().GetResult();
        }

        public async Task<IList<IPersonProperties>> GetPeopleFollowedByAsync(string accountName, params Expression<Func<IPersonProperties, object>>[] selectors)
        {
            var baseUrl = $"_api/SP.UserProfiles.PeopleManager/getpeoplefollowedby(accountName='{HttpUtility.UrlEncode(accountName)}')";

            return await GetGenericPeopleManagerResultsAsync(baseUrl, selectors).ConfigureAwait(false);
        }

        public async Task<bool> AmIFollowedByAsync(string accountName)
        {
            var methodName = "AmIFollowedBy";
            var baseUrl = $"_api/SP.UserProfiles.PeopleManager/{methodName}(accountName='{HttpUtility.UrlEncode(accountName)}')";

            return bool.Parse(await GetSingleResult(baseUrl, methodName).ConfigureAwait(false));
        }

        public bool AmIFollowedBy(string accountName)
        {
            return AmIFollowedByAsync(accountName).GetAwaiter().GetResult();
        }

        public async Task<bool> AmIFollowingAsync(string accountName)
        {
            var methodName = "AmIFollowing";
            var baseUrl = $"_api/SP.UserProfiles.PeopleManager/{methodName}(accountName='{HttpUtility.UrlEncode(accountName)}')";

            return bool.Parse(await GetSingleResult(baseUrl, methodName).ConfigureAwait(false));
        }

        public bool AmIFollowing(string accountName)
        {
            return AmIFollowingAsync(accountName).GetAwaiter().GetResult();
        }

        public SocialFollowResult Follow(FollowData request)
        {
            return FollowAsync(request).GetAwaiter().GetResult();
        }

        public async Task<SocialFollowResult> FollowAsync(FollowData request)
        {
            var methodName = "Follow";
            var baseUrl = $"{Uri}/{methodName}{BuildFollowUrl(request)}";

            var result = int.Parse(await GetSingleResult(baseUrl, methodName).ConfigureAwait(false));

            return (SocialFollowResult)result;
        }

        public void StopFollowing(FollowData request)
        {
            StopFollowingAsync(request).GetAwaiter().GetResult();
        }

        public async Task StopFollowingAsync(FollowData request)
        {
            var methodName = "StopFollowing";
            var baseUrl = $"{Uri}/{methodName}{BuildFollowUrl(request)}";

            await GetSingleResult(baseUrl, methodName).ConfigureAwait(false);
        }

        public bool IsFollowed(FollowData request)
        {
            return IsFollowedAsync(request).GetAwaiter().GetResult();
        }

        public async Task<bool> IsFollowedAsync(FollowData request)
        {
            var methodName = "IsFollowed";
            var baseUrl = $"{Uri}/{methodName}{BuildFollowUrl(request)}";

            return bool.Parse(await GetSingleResult(baseUrl, methodName).ConfigureAwait(false));
        }

        public IList<ISocialActor> FollowedByMe(SocialActorTypes types)
        {
            return FollowedByMeAsync(types).GetAwaiter().GetResult();
        }

        public async Task<IList<ISocialActor>> FollowedByMeAsync(SocialActorTypes types)
        {
            var baseUrl = $"{Uri}/my/Followed(types={(int)types})";
            var apiCall = new ApiCall(baseUrl, ApiType.SPORest)
            {
                Interactive = true
            };

            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            return (await SocialBase<IFollowing>.ParseResultsAsync<SocialActor>(response.Json, root => root.Get("d")?.Get("Followed")?.Get("results")).ConfigureAwait(false)).Cast<ISocialActor>().ToList();
        }

        public int FollowedByMeCount(SocialActorTypes types)
        {
            return FollowedByMeCountAsync(types).GetAwaiter().GetResult();
        }

        public async Task<int> FollowedByMeCountAsync(SocialActorTypes types)
        {
            var methodName = "FollowedCount";
            var baseUrl = $"{Uri}/my/{methodName}(types={(int)types})";
            var apiCall = new ApiCall(baseUrl, ApiType.SPORest)
            {
                Interactive = true
            };

            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Json);

            var result = json.Get("d")?.Get(methodName);

            if (result == null)
            {
                throw new ClientException(PnPCoreResources.Exception_Json_Unexpected);
            }

            return result.Value.GetInt32();
        }

        public IList<ISocialActor> MyFollowers()
        {
            return MyFollowersAsync().GetAwaiter().GetResult();
        }

        public async Task<IList<ISocialActor>> MyFollowersAsync()
        {
            var baseUrl = $"{Uri}/my/Followers";
            var apiCall = new ApiCall(baseUrl, ApiType.SPORest)
            {
                Interactive = true
            };

            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            return (await SocialBase<IFollowing>.ParseResultsAsync<SocialActor>(response.Json, root => root.Get("d")?.Get("Followers")?.Get("results")).ConfigureAwait(false)).Cast<ISocialActor>().ToList();
        }

        public IList<ISocialActor> MySuggestions()
        {
            return MySuggestionsAsync().GetAwaiter().GetResult();
        }

        public async Task<IList<ISocialActor>> MySuggestionsAsync()
        {
            var baseUrl = $"{Uri}/my/Suggestions";
            var apiCall = new ApiCall(baseUrl, ApiType.SPORest)
            {
                Interactive = true
            };

            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            return (await SocialBase<IFollowing>.ParseResultsAsync<SocialActor>(response.Json, root => root.Get("d")?.Get("Suggestions")?.Get("results")).ConfigureAwait(false)).Cast<ISocialActor>().ToList();
        }

        private string BuildFollowUrl(FollowData request)
        {
            var baseUrl = $"(ActorType={(int)request.ActorType}";

            if (request is FollowSiteData siteRequest)
            {
                return $"{baseUrl},ContentUri=@v)?@v='{siteRequest.ContentUri}'";
            }

            if (request is FollowDocumentData documentRequest)
            {
                return $"{baseUrl},ContentUri=@v)?@v='{documentRequest.ContentUri}'";
            }

            if (request is FollowPersonData personRequest)
            {
                return $"{baseUrl},AccountName=@v)?@v='{personRequest.AccountName}'";
            }

            if (request is FollowTagData tagRequest)
            {
                return $"{baseUrl},TagGuid='{tagRequest.TagGuid}')";
            }

            throw new ClientException(PnPCoreResources.Exception_Unsupported_FollowRequest);
        }
    }
}
