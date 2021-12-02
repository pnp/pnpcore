using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// An entry point to the social following APIs
    /// </summary>
    [ConcreteType(typeof(Following))]
    public interface IFollowing : IDataModel<IFollowing>
    {
        /// <summary>
        /// Gets the people who are following the specified user.
        /// </summary>
        /// <param name="accountName">The account name of the user, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <param name="selectors">Specific properties to return</param>
        /// <returns>An instance of <see cref="IPersonProperties"/></returns>
        Task<IList<IPersonProperties>> GetFollowersForAsync(string accountName, params Expression<Func<IPersonProperties, object>>[] selectors);

        /// <summary>
        /// Gets the people who are following the specified user.
        /// </summary>
        /// <param name="accountName">The account name of the user, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <param name="selectors">Specific properties to return</param>
        /// <returns>An instance of <see cref="IPersonProperties"/></returns>
        IList<IPersonProperties> GetFollowersFor(string accountName, params Expression<Func<IPersonProperties, object>>[] selectors);

        /// <summary>
        /// Gets the people who the specified user is following.
        /// </summary>
        /// <param name="accountName">The account name of the user, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <param name="selectors">Specific properties to return</param>
        /// <returns>An instance of <see cref="IPersonProperties"/></returns>
        Task<IList<IPersonProperties>> GetPeopleFollowedByAsync(string accountName, params Expression<Func<IPersonProperties, object>>[] selectors);

        /// <summary>
        /// Gets the people who the specified user is following.
        /// </summary>
        /// <param name="accountName">The account name of the user, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <param name="selectors">Specific properties to return</param>
        /// <returns>An instance of <see cref="IPersonProperties"/></returns>
        IList<IPersonProperties> GetPeopleFollowedBy(string accountName, params Expression<Func<IPersonProperties, object>>[] selectors);

        /// <summary>
        /// Checks whether the current user is following the specified user.
        /// </summary>
        /// <param name="accountName">The account name of the user, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <returns><i>True</i> if the current user follows the specified user</returns>
        Task<bool> AmIFollowingAsync(string accountName);

        /// <summary>
        /// Checks whether the current user is following the specified user.
        /// </summary>
        /// <param name="accountName">The account name of the user, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <returns><i>True</i> if the current user follows the specified user</returns>
        bool AmIFollowing(string accountName);

        /// <summary>
        /// Checks whether the specified user is following the current user.
        /// </summary>
        /// <param name="accountName">The account name of the user, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <returns><i>True</i> if the specified user follows the current user</returns>
        Task<bool> AmIFollowedByAsync(string accountName);

        /// <summary>
        /// Checks whether the specified user is following the current user.
        /// </summary>
        /// <param name="accountName">The account name of the user, i.e. "i:0#.f|membership|admin@m365x790252.onmicrosoft.com"</param>
        /// <returns><i>True</i> if the specified user follows the current user</returns>
        bool AmIFollowedBy(string accountName);

        /// <summary>
        /// Gets following information for the current user
        /// </summary>
        /// <returns>An instance of <see cref="IFollowingInfo"/></returns>
        Task<IFollowingInfo> GetFollowingInfoAsync();

        /// <summary>
        /// Gets following information for the current user
        /// </summary>
        /// <returns>An instance of <see cref="IFollowingInfo"/></returns>
        IFollowingInfo GetFollowingInfo();

        /// <summary>
        /// Makes the current user start following a user, document, site, or tag.
        /// </summary>
        /// <param name="data">Depending on what entity your're going to follow, can be an instance of <see cref="FollowTagData"/>, <see cref="FollowDocumentData"/>, <see cref="FollowPersonData"/> or <see cref="FollowSiteData"/></param>
        /// <returns>A result, the follow status or an error</returns>
        SocialFollowResult Follow(FollowData data);

        /// <summary>
        /// Makes the current user start following a user, document, site, or tag.
        /// </summary>
        /// <param name="data">Depending on what entity your're going to follow, can be an instance of <see cref="FollowTagData"/>, <see cref="FollowDocumentData"/>, <see cref="FollowPersonData"/> or <see cref="FollowSiteData"/></param>
        /// <returns>A result, the follow status or an error</returns>
        Task<SocialFollowResult> FollowAsync(FollowData data);

        /// <summary>
        /// Makes the current user stop following a user, document, site, or tag.
        /// </summary>
        /// <param name="data">Depending on what entity your're going to stop following, can be an instance of <see cref="FollowTagData"/>, <see cref="FollowDocumentData"/>, <see cref="FollowPersonData"/> or <see cref="FollowSiteData"/></param>
        void StopFollowing(FollowData data);

        /// <summary>
        /// Makes the current user stop following a user, document, site, or tag.
        /// </summary>
        /// <param name="data">Depending on what entity your're going to stop following, can be an instance of <see cref="FollowTagData"/>, <see cref="FollowDocumentData"/>, <see cref="FollowPersonData"/> or <see cref="FollowSiteData"/></param>
        Task StopFollowingAsync(FollowData data);

        /// <summary>
        /// Indicates whether the current user is following a specified user, document, site, or tag.
        /// </summary>
        /// <param name="data">Depending on what entity your're going to stop following, can be an instance of <see cref="FollowTagData"/>, <see cref="FollowDocumentData"/>, <see cref="FollowPersonData"/> or <see cref="FollowSiteData"/></param>
        /// <returns><i>True</i> if the current user is following a specified user, document, site, or tag.</returns>
        bool IsFollowed(FollowData data);

        /// <summary>
        /// Indicates whether the current user is following a specified user, document, site, or tag.
        /// </summary>
        /// <param name="data">Depending on what entity your're going to stop following, can be an instance of <see cref="FollowTagData"/>, <see cref="FollowDocumentData"/>, <see cref="FollowPersonData"/> or <see cref="FollowSiteData"/></param>
        /// <returns><i>True</i> if the current user is following a specified user, document, site, or tag.</returns>
        Task<bool> IsFollowedAsync(FollowData data);

        /// <summary>
        /// Gets users, documents, sites, and tags that the current user is following.
        /// </summary>
        /// <param name="types">The actor type to include. You can include many actory using bitwise operatoins. I.e. to include document and site types use <i>SocialActorTypes.Documents | SocialActorTypes.Sites</i></param>
        /// <returns>A collection of <see cref="ISocialActor"/></returns>
        IList<ISocialActor> FollowedByMe(SocialActorTypes types);

        /// <summary>
        /// Gets users, documents, sites, and tags that the current user is following.
        /// </summary>
        /// <param name="types">The actor type to include. You can include many actory using bitwise operatoins. I.e. to include document and site types use <i>SocialActorTypes.Documents | SocialActorTypes.Sites</i></param>
        /// <returns>A collection of <see cref="ISocialActor"/></returns>
        Task<IList<ISocialActor>> FollowedByMeAsync(SocialActorTypes types);

        /// <summary>
        /// Gets the count of users, documents, sites, and tags that the current user is following.
        /// </summary>
        /// <param name="types">The actor type to include. You can include many actory using bitwise operatoins. I.e. to include document and site types use <i>SocialActorTypes.Documents | SocialActorTypes.Sites</i></param>
        /// <returns>The number of followed items</returns>
        int FollowedByMeCount(SocialActorTypes types);

        /// <summary>
        /// Gets the count of users, documents, sites, and tags that the current user is following.
        /// </summary>
        /// <param name="types">The actor type to include. You can include many actory using bitwise operatoins. I.e. to include document and site types use <i>SocialActorTypes.Documents | SocialActorTypes.Sites</i></param>
        /// <returns>The number of followed items</returns>
        Task<int> FollowedByMeCountAsync(SocialActorTypes types);

        /// <summary>
        /// Gets the users who are following the current user.
        /// </summary>
        /// <returns>A collection of <see cref="ISocialActor"/></returns>
        IList<ISocialActor> MyFollowers();

        /// <summary>
        /// Gets the users who are following the current user.
        /// </summary>
        /// <returns>A collection of <see cref="ISocialActor"/></returns>
        Task<IList<ISocialActor>> MyFollowersAsync();

        /// <summary>
        /// Gets users who the current user might want to follow.
        /// </summary>
        /// <returns>A collection of <see cref="ISocialActor"/></returns>
        IList<ISocialActor> MySuggestions();

        /// <summary>
        /// Gets users who the current user might want to follow.
        /// </summary>
        /// <returns>A collection of <see cref="ISocialActor"/></returns>
        Task<IList<ISocialActor>> MySuggestionsAsync();
    }
}
